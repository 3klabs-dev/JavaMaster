// Study mode pop-up: choose Daily Challenge, Courses or Multiplayer, then go to that page
(function () {
    // Every "Study" entry point on the page opens the same pop-up: the sidebar
    // button, and the "Study today" button on Progress.
    const studyBtns = document.querySelectorAll('.study-button');
    if (!studyBtns.length) return;   // page without the sidebar

    let lastTrigger = studyBtns[0];

    const overlay = document.createElement('div');
    overlay.className = 'study-overlay';
    overlay.innerHTML = `
        <div class="study-modal" role="dialog" aria-modal="true" aria-labelledby="studyModalTitle">
            <h2 id="studyModalTitle" class="visually-hidden">Choose a study mode</h2>

            <div class="study-options" id="studyOptions">
                <button type="button" class="study-option" data-target="Daily Challenges.html" aria-pressed="false">
                    <img src="/icons/target.png" alt="">
                    <span class="study-option-title">Daily Challenge</span>
                    <span class="study-option-sub">Solve today's puzzle</span>
                </button>

                <button type="button" class="study-option" data-target="Learn.html" aria-pressed="false">
                    <img src="/icons/programming.png" alt="">
                    <span class="study-option-title">Courses</span>
                    <span class="study-option-sub">learn various topics</span>
                </button>

                <button type="button" class="study-option" data-target="Lobby.html" aria-pressed="false">
                    <img src="/icons/multiplayer.png" alt="">
                    <span class="study-option-title">Multiplayer</span>
                    <span class="study-option-sub">Play with your friends</span>
                </button>
            </div>

            <button type="button" class="study-start" id="studyStart">Start Learning</button>
            <button type="button" class="study-cancel" id="studyCancel">Cancel</button>
        </div>
    `;
    document.body.appendChild(overlay);

    const options = overlay.querySelectorAll('.study-option');
    let selected = null;

    function select(option) {
        selected = option;
        options.forEach(function (o) {
            const on = o === option;
            o.classList.toggle('selected', on);
            o.setAttribute('aria-pressed', String(on));
        });
    }

    function openModal(trigger) {
        lastTrigger = trigger || studyBtns[0];
        select(null);                       // always start with nothing selected
        overlay.classList.add('active');
        options[0].focus();
    }

    // Closing is instant (no fade-out), so a restored page can never show a fading ghost
    function closeModal() {
        overlay.classList.remove('active');
    }

    // Open from any Study button (sidebar, Progress "Study today")
    studyBtns.forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            openModal(btn);
        });
    });

    // Pick a mode
    options.forEach(function (o) {
        o.addEventListener('click', function () { select(o); });
    });

    // Start Learning: go to the chosen page (does nothing until a mode is picked)
    overlay.querySelector('#studyStart').addEventListener('click', function () {
        if (!selected) return;
        window.location.href = selected.dataset.target;
    });

    // Three ways to close: Cancel button, click outside the window, Escape key
    overlay.querySelector('#studyCancel').addEventListener('click', function () {
        closeModal();
        lastTrigger.focus();
    });

    overlay.addEventListener('click', function (e) {
        if (e.target === overlay) closeModal();
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && overlay.classList.contains('active')) {
            closeModal();
            lastTrigger.focus();
        }
    });

    // Coming back from the back/forward cache: close the pop-up it was saved with
    window.addEventListener('pageshow', function (e) {
        if (e.persisted) closeModal();
    });
})();