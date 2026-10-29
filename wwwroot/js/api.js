async function api(path, options = {}) {
    const res = await fetch('/api' + path, {
        headers: { 'Content-Type': 'application/json' },
        ...options
    });
    const data = await res.json().catch(() => ({}));
    if (!res.ok) {
        const err = new Error(data.error || 'Something went wrong.');
        err.status = res.status;
        err.data = data;
        throw err;
    }
    return data;
}

const JM = {
    me:          ()               => api('/me'),
    shop:        ()               => api('/shop'),
    buy:         (code, packSize) => api('/shop/buy', { method: 'POST', body: JSON.stringify({ code, packSize }) }),
    daily:       ()               => api('/daily'),
    dailyAnswer: (answer)         => api('/daily/answer', { method: 'POST', body: JSON.stringify({ answer }) }),
    repair:      ()               => api('/streak/repair', { method: 'POST' }),
    saveProfile: (profile)        => api('/me/profile', { method: 'PATCH', body: JSON.stringify(profile) })
};

/*
  The chosen avatar belongs to the account, so the navbar reads it from the
  server instead of from the browser. The profile page sets the avatar itself
  the moment it saves one, so it opts out of this.
*/
async function syncNavbarAvatar() {
    const el = document.getElementById('navbarAvatar');
    if (!el) return;
    try {
        const me = await JM.me();
        if (me && me.avatar) el.src = me.avatar;
    } catch (e) {
        console.error('Could not load the avatar from the server', e);
    }
}

window.addEventListener('DOMContentLoaded', function () {
    if (window.JM_OWNS_AVATAR) return;
    syncNavbarAvatar();
});
