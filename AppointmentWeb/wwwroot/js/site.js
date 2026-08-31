// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    const menuToggle = document.getElementById('menu-toggle');
    const wrapper = document.getElementById('wrapper');

    if (menuToggle && wrapper) {
        menuToggle.addEventListener('click', function (e) {
            e.preventDefault();
            wrapper.classList.toggle('toggled');
        });

        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', function (e) {
            if (window.innerWidth <= 768 && wrapper.classList.contains('toggled')) {
                if (!wrapper.querySelector('#sidebar-wrapper').contains(e.target) &&
                    !menuToggle.contains(e.target)) {
                    wrapper.classList.remove('toggled');
                }
            }
        });
    }
});

/* ==========================================
   SWEETALERT GLOBAL CONFIRM FUNCTION
   Tüm projede standart confirm() yerine kullanılır
========================================== */
window.confirmFormSubmit = function (event, message) {
    event.preventDefault();
    
    const form = event.currentTarget.tagName === 'FORM' ? event.currentTarget : event.target.closest('form');
    if (!form) return;

    Swal.fire({
        title: 'Emin misiniz?',
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Evet',
        cancelButtonText: 'Vazgeç',
        customClass: {
            confirmButton: 'swal2-confirm',
            cancelButton: 'swal2-cancel'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            form.submit();
        }
    });
};

/* ==========================================
   THEME SYSTEM MANAGER
========================================== */
window.setTheme = function(themeName) {
    // 1. DOM'a uygula
    document.documentElement.setAttribute('data-theme', themeName);
    
    // 2. Tarayıcıya kaydet
    const storageKey = window.themeStorageKey || 'randevuplus-theme-global-last';
    localStorage.setItem(storageKey, themeName);
    
    // Ayrıca çıkış yapıldığında (Login sayfası) son temayı hatırlaması için globale de kaydet
    localStorage.setItem('randevuplus-theme-global-last', themeName);
    
    // 3. Varsa UI kartlarını güncelle
    updateThemeSelectionUI(themeName);
};

function updateThemeSelectionUI(themeName) {
    const themeCards = document.querySelectorAll('.theme-selector-card');
    if (themeCards.length === 0) return; // Settings sayfasında değilsek çık

    themeCards.forEach(card => {
        if (card.getAttribute('data-theme-name') === themeName) {
            card.classList.add('active-theme', 'border-primary', 'shadow');
            const checkBadge = card.querySelector('.theme-active-badge');
            if(checkBadge) checkBadge.classList.remove('d-none');
        } else {
            card.classList.remove('active-theme', 'border-primary', 'shadow');
            const checkBadge = card.querySelector('.theme-active-badge');
            if(checkBadge) checkBadge.classList.add('d-none');
        }
    });
}

// Sayfa yüklendiğinde aktif temayı UI'da seçili göster
document.addEventListener('DOMContentLoaded', function() {
    const storageKey = window.themeStorageKey || 'randevuplus-theme-global-last';
    const savedTheme = localStorage.getItem(storageKey) || localStorage.getItem('randevuplus-theme-global-last') || 'classic';
    updateThemeSelectionUI(savedTheme);
});

/* ==========================================
   DARK MODE TOGGLE LOGIC (DECOUPLED)
========================================== */
window.toggleDarkMode = function() {
    const currentMode = document.documentElement.getAttribute('data-mode') || 'light';
    const storageKey = window.themeStorageKey || 'randevuplus-theme-global-last';
    const modeKey = storageKey + '-mode';

    if (currentMode === 'dark') {
        // Karanlık moddaysak, aydınlığa dön
        document.documentElement.setAttribute('data-mode', 'light');
        localStorage.setItem(modeKey, 'light');
        localStorage.setItem('randevuplus-theme-global-last-mode', 'light');
    } else {
        // Karanlık modda değilsek, karanlığa geç
        document.documentElement.setAttribute('data-mode', 'dark');
        localStorage.setItem(modeKey, 'dark');
        localStorage.setItem('randevuplus-theme-global-last-mode', 'dark');
    }
};
