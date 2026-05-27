document.addEventListener('DOMContentLoaded', () => {
  const toggle = document.querySelector('.nav-toggle');
  const menu = document.querySelector('.nav-menu');
  if (toggle && menu) toggle.addEventListener('click', () => menu.classList.toggle('open'));

  document.querySelectorAll('[data-confirm]').forEach(btn => {
    btn.addEventListener('click', e => {
      if (!confirm(btn.getAttribute('data-confirm'))) e.preventDefault();
    });
  });

  const avatarInput = document.querySelector('.avatar-input');
  const preview = document.getElementById('avatarPreview');
  if (avatarInput && preview) {
    avatarInput.addEventListener('change', () => {
      const file = avatarInput.files && avatarInput.files[0];
      if (file) preview.src = URL.createObjectURL(file);
    });
  }
});
