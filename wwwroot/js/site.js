// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Tag badge logic for Admin Content Create/Edit
window.initAdminTagBadges = function(formSelector) {
    const form = document.querySelector(formSelector);
    if (!form) return;
    const tagInput = form.querySelector('#tag-input');
    const tagBadges = form.querySelector('#tag-badges');
    const tagNamesInput = form.querySelector('#TagNames');
    if (!tagInput || !tagBadges || !tagNamesInput) return;
    let tags = [];
    // Initial value (for Edit)
    if (tagNamesInput.value) {
        tags = tagNamesInput.value.split(',').map(t => t.trim()).filter(t => t);
    }
    function renderTags() {
        tagBadges.innerHTML = '';
        tags.forEach((tag, idx) => {
            const wrapper = document.createElement('span');
            wrapper.className = 'inline-flex items-center gap-0 mr-2 mb-1';
            const badge = document.createElement('span');
            badge.className = 'badge bg-lime-600 text-white px-2 py-1 rounded-full font-medium';
            badge.textContent = tag;
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.setAttribute('aria-label', 'Remove');
            btn.setAttribute('data-idx', idx);
            btn.className = 'text-white hover:text-red-400 focus:outline-none cursor-pointer flex items-center justify-center';
            btn.style.fontSize = '1em';
            btn.innerHTML = "<i class='bi bi-x text-base align-middle'></i>";
            wrapper.appendChild(badge);
            wrapper.appendChild(btn);
            tagBadges.appendChild(wrapper);
            // Debug log
            console.log('Badge:', wrapper.outerHTML);
        });
        tagNamesInput.value = tags.join(',');
    }
    renderTags();
    tagInput.onkeydown = function(e) {
        if ((e.key === 'Enter' || e.key === ',') && !e.shiftKey) {
            e.preventDefault();
            let val = tagInput.value.trim();
            if (val && !tags.includes(val)) {
                tags.push(val);
                renderTags();
            }
            tagInput.value = '';
        }
    };
    tagBadges.addEventListener('click', function(e) {
        const btn = e.target.closest('button[data-idx]');
        if (btn) {
            const idx = btn.getAttribute('data-idx');
            tags.splice(idx, 1);
            renderTags();
        }
    });
    form.addEventListener('submit', function(e) {
        let val = tagInput.value.trim();
        if (val && !tags.includes(val)) {
            tags.push(val);
            renderTags();
            tagInput.value = '';
        }
    });
};
// Usage: window.initAdminTagBadges('#content-form');
// You should call this after modal is shown and form is in DOM.
