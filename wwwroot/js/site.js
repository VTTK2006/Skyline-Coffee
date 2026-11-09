document.addEventListener('DOMContentLoaded', () => {
  const cartAddScrollKey = 'cart-add-scroll-position';
  const savedCartScroll = sessionStorage.getItem(cartAddScrollKey);
  if (savedCartScroll) {
    sessionStorage.removeItem(cartAddScrollKey);
    try {
      const savedPosition = JSON.parse(savedCartScroll);
      const currentPath = window.location.pathname + window.location.search;
      if (savedPosition.path === currentPath && Number.isFinite(savedPosition.scrollY)) {
        requestAnimationFrame(() => window.scrollTo(0, savedPosition.scrollY));
      }
    } catch {
      sessionStorage.removeItem(cartAddScrollKey);
    }
  }

  document.querySelectorAll('form').forEach(form => {
    if (new URL(form.action, window.location.href).pathname !== '/Cart/Add') return;

    form.addEventListener('submit', () => {
      const returnUrl = form.querySelector('input[name="returnUrl"]')?.value;
      const currentPath = window.location.pathname + window.location.search;
      if (returnUrl === currentPath) {
        sessionStorage.setItem(cartAddScrollKey, JSON.stringify({ path: returnUrl, scrollY: window.scrollY }));
      }
    });
  });

  const toggle = document.querySelector('.nav-toggle');
  const menu = document.querySelector('.nav-menu');
  if (toggle && menu) {
    toggle.addEventListener('click', () => {
      const isOpen = menu.classList.toggle('open');
      toggle.setAttribute('aria-expanded', String(isOpen));
    });
    menu.querySelectorAll('a').forEach(link => link.addEventListener('click', () => {
      menu.classList.remove('open');
      toggle.setAttribute('aria-expanded', 'false');
    }));

    const navLinks = menu.querySelectorAll('.nav-links a');
    navLinks.forEach(link => {
      link.addEventListener('click', () => {
        navLinks.forEach(item => item.classList.remove('active'));
        link.classList.add('active');
      });
    });
  }

  const account = document.querySelector('.account-dropdown');
  const accountButton = document.querySelector('.account-button');
  if (account && accountButton) {
    accountButton.addEventListener('click', () => {
      const isOpen = account.classList.toggle('open');
      accountButton.setAttribute('aria-expanded', String(isOpen));
    });
    document.addEventListener('click', event => {
      if (!account.contains(event.target)) {
        account.classList.remove('open');
        accountButton.setAttribute('aria-expanded', 'false');
      }
    });
  }

  const sizeSelect = document.querySelector('.detail-form select[name="size"]');
  const toppingSelect = document.querySelector('.detail-form select[name="topping"]');
  const selectedPrice = document.querySelector('[data-selected-price]');
  if (sizeSelect && toppingSelect && selectedPrice) {
    const updateSelectedPrice = () => {
      const sizePrice = Number(sizeSelect.selectedOptions[0].dataset.price);
      const toppingPrice = Number(toppingSelect.selectedOptions[0].dataset.extraPrice);
      selectedPrice.textContent = `${new Intl.NumberFormat('vi-VN').format(sizePrice + toppingPrice)}đ`;
    };

    sizeSelect.addEventListener('change', updateSelectedPrice);
    toppingSelect.addEventListener('change', updateSelectedPrice);
  }

  document.addEventListener('keydown', event => {
    if (event.key === 'Escape' && menu) {
      menu.classList.remove('open');
      toggle?.setAttribute('aria-expanded', 'false');
    }
  });

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

  // Hero Banner Slider
  const heroSlider = document.querySelector('.hero-slider');
  if (heroSlider) {
    const slides = heroSlider.querySelectorAll('.hero-slide');
    const dots = heroSlider.querySelectorAll('.slider-dot');
    const prevBtn = heroSlider.querySelector('.slider-btn.prev');
    const nextBtn = heroSlider.querySelector('.slider-btn.next');
    let currentIndex = 0;
    let autoSlideInterval = null;
    const slideDuration = 4500; // 4.5 seconds

    function goToSlide(index) {
      if (!slides.length) return;
      if (index < 0) {
        currentIndex = slides.length - 1;
      } else if (index >= slides.length) {
        currentIndex = 0;
      } else {
        currentIndex = index;
      }

      slides.forEach((slide, i) => {
        const isActive = i === currentIndex;
        slide.classList.toggle('active', isActive);
      });

      dots.forEach((dot, i) => {
        const isActive = i === currentIndex;
        dot.classList.toggle('active', isActive);
        dot.setAttribute('aria-selected', String(isActive));
      });
    }

    function nextSlide() {
      goToSlide(currentIndex + 1);
    }

    function prevSlide() {
      goToSlide(currentIndex - 1);
    }

    function startAutoSlide() {
      stopAutoSlide();
      autoSlideInterval = setInterval(nextSlide, slideDuration);
    }

    function stopAutoSlide() {
      if (autoSlideInterval) {
        clearInterval(autoSlideInterval);
        autoSlideInterval = null;
      }
    }

    if (nextBtn) {
      nextBtn.addEventListener('click', () => {
        nextSlide();
        startAutoSlide();
      });
    }

    if (prevBtn) {
      prevBtn.addEventListener('click', () => {
        prevSlide();
        startAutoSlide();
      });
    }

    dots.forEach((dot, index) => {
      dot.addEventListener('click', () => {
        goToSlide(index);
        startAutoSlide();
      });
    });

    // Pause autoplay on mouse enter, resume on mouse leave
    heroSlider.addEventListener('mouseenter', stopAutoSlide);
    heroSlider.addEventListener('mouseleave', startAutoSlide);

    // Touch swipe support for mobile
    let touchStartX = 0;
    let touchEndX = 0;

    heroSlider.addEventListener('touchstart', e => {
      if (e.changedTouches && e.changedTouches[0]) {
        touchStartX = e.changedTouches[0].screenX;
      }
      stopAutoSlide();
    }, { passive: true });

    heroSlider.addEventListener('touchend', e => {
      if (e.changedTouches && e.changedTouches[0]) {
        touchEndX = e.changedTouches[0].screenX;
        const diff = touchEndX - touchStartX;
        if (Math.abs(diff) > 40) {
          if (diff < 0) {
            nextSlide();
          } else {
            prevSlide();
          }
        }
      }
      startAutoSlide();
    }, { passive: true });

    // Start auto slide
    startAutoSlide();
  }
});
