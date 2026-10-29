// ===== PROFILE PAGE LOGIC =====
// (dropdown, logout and notification badge live in profile-dropdown.js)

// The profile belongs to the account, so it is read from the server and
// written back to the server. Nothing about it is kept in the browser.

// School database with branches
const schoolDatabase = {
    "STI": ["Alabang Campus", "Bacoor Campus", "Calamba Campus", "Caloocan Campus", "Cavite Campus", "Cubao Campus", "Dasmarinas Campus", "Fairview Campus", "Las Piñas Campus", "Lipa Campus", "Lucena Campus", "Makati Campus", "Manila Campus", "Novaliches Campus", "Ortigas Campus", "Quezon Avenue Campus", "Recto Campus", "Rosario Campus", "San Fernando Pampanga Campus", "Sta. Rosa Campus", "Taft Campus"],
};

// Rank thresholds (the same XP ranges the server uses). These are only the
// fallback for when the server cannot be reached.
const ranks = [
    { min: 6500, name: 'Legend' },
    { min: 4500, name: 'Boss' },
    { min: 3000, name: 'Elite' },
    { min: 1800, name: 'Pro' },
    { min: 1200, name: 'Skilled' },
    { min: 700,  name: 'Rising' },
    { min: 300,  name: 'Beginner' },
    { min: 0,    name: 'Starter' }
];

// The profile page owns the avatar: it sets the navbar itself after every save,
// so the shared navbar sync in api.js keeps out of the way.
window.JM_OWNS_AVATAR = true;

// The profile, exactly as the server has it
let currentAvatar = '/avatars/Avatar 1.png';
let selectedAvatar = currentAvatar;
let userName = '';
let userSchool = '';
let userGrade = '';
let joinedDate = null;
let bestStreak = 0;

// Progress data. It comes from the server, so the profile can never show
// an XP or streak number the API disagrees with.
let streakDays = 0;
let totalXP = 0;
let rankInfo = null;

// Current rank name + icon from XP
function getCurrentRank() {
    if (rankInfo) return { name: rankInfo.name, iconPath: rankInfo.icon };

    const rank = ranks.find(function (r) { return totalXP >= r.min; });
    return {
        name: rank.name,
        iconPath: '/rank titles/' + rank.name.toLowerCase() + '.png'
    };
}

// "September 23, 2025" (also understands the older "23 Sep 2025" format)
function formatJoinedDate(value) {
    if (!value) return '';
    const date = new Date(value);
    if (isNaN(date)) return value;
    return date.toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });
}

// A field that has not been filled in shows a placeholder, not an empty line.
function orNotSet(value) {
    return value || 'Not set yet';
}

function updateNavbarAvatar() {
    const navbarAvatarElement = document.getElementById('navbarAvatar');
    if (navbarAvatarElement) {
        navbarAvatarElement.src = currentAvatar;
    }
}

// Fill in everything on the profile view
function initializeProfile() {
    const rank = getCurrentRank();

    document.getElementById('profileAvatar').querySelector('img').src = currentAvatar;
    document.getElementById('profileName').textContent = userName;
    document.getElementById('profileSchool').textContent = orNotSet(userSchool);
    document.getElementById('profileGrade').textContent = orNotSet(userGrade);
    document.getElementById('joinedDate').textContent = formatJoinedDate(joinedDate);

    document.getElementById('rankTitle').textContent = rank.name;
    document.getElementById('rankIconProfile').src = rank.iconPath;
    document.getElementById('bestStreak').textContent = bestStreak + (bestStreak === 1 ? ' Day' : ' Days');

    updateNavbarAvatar();
}

// ===== Tabs (Stats / Feed) =====
function showTab(name) {
    document.querySelectorAll('.tab-btn').forEach(function (btn) {
        const isActive = btn.dataset.tab === name;
        btn.classList.toggle('active', isActive);
        btn.setAttribute('aria-selected', isActive);
    });
    document.getElementById('statsPanel').hidden = (name !== 'stats');
    document.getElementById('feedPanel').hidden = (name !== 'feed');
}

document.querySelectorAll('.tab-btn').forEach(function (btn) {
    btn.addEventListener('click', function () {
        showTab(btn.dataset.tab);
    });
});

// ===== Edit profile =====

// The school list is fixed, but a learner can be at a school that is not on it
// (the onboarding page lets them type one). A value that is not in the list is
// added to it, so opening the edit view never quietly throws the school away.
function populateSchoolDropdown() {
    const schoolSelect = document.getElementById('schoolInput');
    schoolSelect.innerHTML = '<option value="">Select school</option>';

    Object.keys(schoolDatabase).sort().forEach(function (schoolName) {
        schoolDatabase[schoolName].forEach(function (campus) {
            const option = document.createElement('option');
            option.value = schoolName + ' - ' + campus;
            option.textContent = schoolName + ' - ' + campus;
            schoolSelect.appendChild(option);
        });
    });

    if (userSchool && !Array.from(schoolSelect.options).some(function (o) { return o.value === userSchool; })) {
        const option = document.createElement('option');
        option.value = userSchool;
        option.textContent = userSchool;
        schoolSelect.appendChild(option);
    }
}

// The grade list is fixed too, for the same reason.
function populateGradeDropdown() {
    const gradeSelect = document.getElementById('gradeInput');
    const options = Array.from(gradeSelect.options).map(function (o) { return o.value; });

    if (userGrade && options.indexOf(userGrade) === -1) {
        const option = document.createElement('option');
        option.value = userGrade;
        option.textContent = userGrade;
        gradeSelect.appendChild(option);
    }
}

function showEditView() {
    document.querySelector('.profile-view').classList.remove('active');
    document.querySelector('.edit-view').classList.add('active');

    populateSchoolDropdown();
    populateGradeDropdown();

    document.getElementById('nameInput').value = userName;
    document.getElementById('schoolInput').value = userSchool;
    document.getElementById('gradeInput').value = userGrade;
    document.getElementById('editAvatar').querySelector('img').src = currentAvatar;
}

function cancelEdit() {
    document.querySelector('.edit-view').classList.remove('active');
    document.querySelector('.profile-view').classList.add('active');
}

async function saveProfile() {
    const name = document.getElementById('nameInput').value.trim();
    const school = document.getElementById('schoolInput').value;
    const grade = document.getElementById('gradeInput').value;

    if (!name || !school || !grade) {
        alert('Please fill in all fields');
        return;
    }

    // The server owns the profile, so the change is saved there first and the
    // page only shows it once the server has taken it.
    try {
        const saved = await JM.saveProfile({
            fullName: name,
            school: school,
            grade: grade,
            avatar: currentAvatar
        });
        userName  = saved.fullName;
        userSchool = saved.school ?? '';
        userGrade  = saved.grade ?? '';
        currentAvatar = saved.avatar ?? currentAvatar;
    } catch (e) {
        alert(e.message);
        return;
    }

    document.getElementById('profileName').textContent = userName;
    document.getElementById('profileSchool').textContent = orNotSet(userSchool);
    document.getElementById('profileGrade').textContent = orNotSet(userGrade);
    document.getElementById('profileAvatar').querySelector('img').src = currentAvatar;

    updateNavbarAvatar();
    cancelEdit();
}

// ===== Avatar modal =====
function showAvatarModal() {
    document.getElementById('avatarModal').classList.add('active');
    selectedAvatar = currentAvatar;
    updateAvatarSelection();
}

function closeAvatarModal() {
    document.getElementById('avatarModal').classList.remove('active');
}

async function confirmAvatar() {
    const avatar = selectedAvatar;

    try {
        const saved = await JM.saveProfile({ avatar: avatar });
        currentAvatar = saved.avatar ?? avatar;
    } catch (e) {
        alert(e.message);
        return;
    }

    document.getElementById('editAvatar').querySelector('img').src = currentAvatar;
    document.getElementById('profileAvatar').querySelector('img').src = currentAvatar;

    updateNavbarAvatar();

    closeAvatarModal();
}

function updateAvatarSelection() {
    document.querySelectorAll('.avatar-option').forEach(function (option) {
        option.classList.toggle('selected', option.dataset.avatar === selectedAvatar);
    });
}

document.getElementById('avatarGrid').addEventListener('click', function (e) {
    const option = e.target.closest('.avatar-option');
    if (option) {
        selectedAvatar = option.dataset.avatar;
        updateAvatarSelection();
    }
});

// ===== Start =====
async function loadPlayer() {
    try {
        const me = await JM.me();
        streakDays = me.streak ?? 0;
        totalXP = me.totalXp ?? 0;
        if (me.rank) rankInfo = { name: me.rank.name, icon: me.rank.icon };

        userName   = me.fullName || '';
        userSchool = me.school || '';
        userGrade  = me.grade || '';
        currentAvatar = me.avatar || currentAvatar;
        selectedAvatar = currentAvatar;
        joinedDate = me.joinedDate || null;
        bestStreak = me.bestStreak ?? 0;
    } catch (e) {
        console.error('Could not load the player from the server', e);
    }
    initializeProfile();
}

loadPlayer();

// Refresh when the player comes back with the Back button
window.addEventListener('pageshow', function (e) {
    if (e.persisted) loadPlayer();
});
