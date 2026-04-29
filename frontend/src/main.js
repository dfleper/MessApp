import './style.css';

const MIN_NAME_LENGTH = 2;
const MAX_NAME_LENGTH = 60;

const MIN_EMAIL_LENGTH = 5;
const MAX_EMAIL_LENGTH = 100;

const MIN_SUBJECT_LENGTH = 2;
const MAX_SUBJECT_LENGTH = 80;

const MIN_MESSAGE_LENGTH = 10;
const MAX_MESSAGE_LENGTH = 250;

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

const form = document.getElementById('form');
const nameInput = document.getElementById('nameInput');
const emailInput = document.getElementById('emailInput');
const subjectInput = document.getElementById('subjectInput');
const messageInput = document.getElementById('messageInput');

if (form && nameInput && emailInput && subjectInput && messageInput) {
  form.addEventListener('submit', async (event) => {
    event.preventDefault();

    const nombre = nameInput.value.trim();
    const email = emailInput.value.trim();
    const asunto = subjectInput.value.trim();
    const mensaje = messageInput.value.trim();

    const nombreValido = validarNombre(nombre);
    const emailValido = validarEmail(email);
    const asuntoValido = validarAsunto(asunto);
    const mensajeValido = validarMensaje(mensaje);

    if (!nombreValido || !emailValido || !asuntoValido || !mensajeValido) {
      return;
    }

    try {
      const response = await fetch(`${API_URL}/api/mensajes`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ nombre, email, asunto, mensaje })
      });

      if (!response.ok) {
        showToast('No se pudo enviar el mensaje. Intenta nuevamente.', 'error');
        return;
      }

      showToast('Mensaje enviado correctamente', 'success');
      form.reset();
    } catch {
      showToast('No se pudo conectar con el backend', 'error');
    }
  });
}

const validarNombre = (nombre) => {
  if (nombre.length === 0) {
    showToast('El nombre es obligatorio', 'error');
    return false;
  }

  if (nombre.length < MIN_NAME_LENGTH) {
    showToast(`El nombre debe tener al menos ${MIN_NAME_LENGTH} caracteres`, 'error');
    return false;
  }

  if (nombre.length > MAX_NAME_LENGTH) {
    showToast(`El nombre no puede superar los ${MAX_NAME_LENGTH} caracteres`, 'error');
    return false;
  }

  if (!/^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$/.test(nombre)) {
    showToast('El nombre solo puede contener letras y espacios', 'error');
    return false;
  }

  return true;
};

const validarEmail = (email) => {
  if (email.length === 0) {
    showToast('El email es obligatorio', 'error');
    return false;
  }

  if (email.length < MIN_EMAIL_LENGTH) {
    showToast(`El email debe tener al menos ${MIN_EMAIL_LENGTH} caracteres`, 'error');
    return false;
  }

  if (email.length > MAX_EMAIL_LENGTH) {
    showToast(`El email no puede superar los ${MAX_EMAIL_LENGTH} caracteres`, 'error');
    return false;
  }

  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    showToast('Introduce un email válido', 'error');
    return false;
  }

  return true;
};

const validarAsunto = (asunto) => {
  if (asunto.length === 0) {
    showToast('El asunto es obligatorio', 'error');
    return false;
  }

  if (asunto.length < MIN_SUBJECT_LENGTH) {
    showToast(`El asunto debe tener al menos ${MIN_SUBJECT_LENGTH} caracteres`, 'error');
    return false;
  }

  if (asunto.length > MAX_SUBJECT_LENGTH) {
    showToast(`El asunto no puede superar los ${MAX_SUBJECT_LENGTH} caracteres`, 'error');
    return false;
  }

  return true;
};

const validarMensaje = (mensaje) => {
  if (mensaje.length === 0) {
    showToast('El mensaje es obligatorio', 'error');
    return false;
  }

  if (mensaje.length < MIN_MESSAGE_LENGTH) {
    showToast(`El mensaje debe tener al menos ${MIN_MESSAGE_LENGTH} caracteres`, 'error');
    return false;
  }

  if (mensaje.length > MAX_MESSAGE_LENGTH) {
    showToast(`El mensaje no puede superar los ${MAX_MESSAGE_LENGTH} caracteres`, 'error');
    return false;
  }

  return true;
};

const toastContainer = document.getElementById('toastContainer');

const activeToasts = new Set();

const showToast = (message, type = 'info') => {
  if (!toastContainer) {
    console.error('No existe #toastContainer');
    return;
  }

  const toastKey = `${type}-${message}`;

  if (activeToasts.has(toastKey)) {
    return;
  }

  activeToasts.add(toastKey);

  const toast = document.createElement('div');

  const toastStyles = {
    info: {
      icon: 'i',
      accent: 'from-cyan-400 to-blue-500',
      border: 'border-cyan-300/40',
      glow: 'shadow-cyan-500/25',
      subtitle: 'Información del formulario'
    },
    success: {
      icon: '✓',
      accent: 'from-emerald-300 to-teal-500',
      border: 'border-emerald-300/40',
      glow: 'shadow-emerald-500/25',
      subtitle: 'Todo parece correcto'
    },
    error: {
      icon: '!',
      accent: 'from-fuchsia-400 to-violet-500',
      border: 'border-fuchsia-300/40',
      glow: 'shadow-fuchsia-500/25',
      subtitle: 'Revisa este campo antes de continuar'
    },
    warning: {
      icon: '⚠',
      accent: 'from-amber-300 to-orange-400',
      border: 'border-amber-300/40',
      glow: 'shadow-amber-500/25',
      subtitle: 'Comprueba este dato'
    }
  };

  const currentToast = toastStyles[type] || toastStyles.info;

  toast.className = `
    pointer-events-auto
    relative w-full overflow-hidden rounded-2xl border
    bg-slate-950/90 px-4 py-3 text-white
    ${currentToast.border}
    shadow-2xl ${currentToast.glow}
    backdrop-blur-xl
    flex items-center gap-4
    animate-[toastIn_0.25s_ease-out]
  `;

  toast.innerHTML = `
    <div class="absolute inset-x-0 top-0 h-[2px] bg-gradient-to-r ${currentToast.accent}"></div>

    <div class="js-toast-icon relative flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-gradient-to-br ${currentToast.accent} font-bold text-white shadow-lg">
    </div>

    <div class="flex-1">
      <p class="js-toast-message text-sm font-semibold tracking-wide">
      </p>
      <p class="js-toast-subtitle mt-0.5 text-xs text-white/55">
      </p>
    </div>

    <button
      type="button"
      class="rounded-full px-2 text-xl leading-none text-white/50 transition hover:bg-white/10 hover:text-white"
      aria-label="Cerrar mensaje"
    >
      ×
    </button>
  `;

  const iconElement = toast.querySelector('.js-toast-icon');
  const messageElement = toast.querySelector('.js-toast-message');
  const subtitleElement = toast.querySelector('.js-toast-subtitle');
  if (!iconElement || !messageElement || !subtitleElement) {
    activeToasts.delete(toastKey);
    return;
  }
  iconElement.textContent = currentToast.icon;
  messageElement.textContent = message;
  subtitleElement.textContent = currentToast.subtitle;

  const removeToast = () => {
    activeToasts.delete(toastKey);
    toast.remove();
  };

  const closeButton = toast.querySelector('button');
  if (!closeButton) {
    activeToasts.delete(toastKey);
    return;
  }

  closeButton.addEventListener('click', removeToast);

  toastContainer.appendChild(toast);

  setTimeout(removeToast, 3500);
};