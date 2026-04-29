const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';
const DEFAULT_LIMIT = 100;

const apiKeyInput = document.getElementById('apiKeyInput');
const adminForm = document.getElementById('adminForm');
const loadButton = document.getElementById('loadButton');
const purgeButton = document.getElementById('purgeButton');
const statusElement = document.getElementById('status');
const messagesBody = document.getElementById('messagesBody');

let currentApiKey = '';

const setStatus = (message) => {
  if (statusElement) {
    statusElement.textContent = message;
  }
};

const getAdminHeaders = () => ({
  'X-Admin-Key': currentApiKey
});

const getApiKey = () => apiKeyInput?.value?.trim() ?? '';

const requireApiKey = () => {
  currentApiKey = getApiKey();
  if (!currentApiKey) {
    setStatus('Debes indicar la API key.');
    return false;
  }

  return true;
};

const formatDate = (value) => {
  if (!value) {
    return '-';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString();
};

const requestAdmin = async (path, options = {}) => {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      ...getAdminHeaders(),
      ...(options.headers ?? {})
    }
  });

  if (!response.ok) {
    let details = `Error ${response.status}`;
    try {
      const body = await response.json();
      if (body?.error) {
        details = body.error;
      }
    } catch {
      // ignore parse error
    }

    throw new Error(details);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
};

const renderMessages = (mensajes) => {
  if (!messagesBody) {
    return;
  }

  messagesBody.innerHTML = '';

  mensajes.forEach((mensaje) => {
    const row = document.createElement('tr');

    const isRead = Boolean(mensaje.readAt);

    row.innerHTML = `
      <td>${mensaje.id}</td>
      <td>${mensaje.nombre}</td>
      <td>${mensaje.email}</td>
      <td>${mensaje.asunto}</td>
      <td>${mensaje.mensaje}</td>
      <td>${formatDate(mensaje.createdAt)}</td>
      <td>${isRead ? 'Sí' : 'No'}</td>
      <td>
        <button type="button" data-action="read" data-id="${mensaje.id}" ${isRead ? 'disabled' : ''}>Marcar leído</button>
        <button type="button" data-action="delete" data-id="${mensaje.id}">Soft delete</button>
      </td>
    `;

    messagesBody.appendChild(row);
  });
};

const loadMessages = async () => {
  if (!requireApiKey()) {
    return;
  }

  setStatus('Cargando mensajes...');

  try {
    const mensajes = await requestAdmin(`/api/mensajes/admin?limit=${DEFAULT_LIMIT}`);
    renderMessages(mensajes ?? []);
    setStatus(`Mensajes cargados: ${(mensajes ?? []).length}`);
  } catch (error) {
    setStatus(`No se pudieron cargar los mensajes: ${error.message}`);
  }
};

const markAsRead = async (id) => {
  setStatus(`Marcando mensaje ${id} como leído...`);

  try {
    await requestAdmin(`/api/mensajes/admin/${id}/read`, { method: 'PATCH' });
    await loadMessages();
  } catch (error) {
    setStatus(`No se pudo marcar como leído: ${error.message}`);
  }
};

const softDelete = async (id) => {
  setStatus(`Haciendo soft delete del mensaje ${id}...`);

  try {
    await requestAdmin(`/api/mensajes/admin/${id}`, { method: 'DELETE' });
    await loadMessages();
  } catch (error) {
    setStatus(`No se pudo hacer soft delete: ${error.message}`);
  }
};

const purgeAll = async () => {
  if (!requireApiKey()) {
    return;
  }

  const confirmed = window.confirm('¿Seguro que quieres borrar TODA la BBDD de mensajes?');
  if (!confirmed) {
    return;
  }

  setStatus('Borrando BBDD de mensajes...');

  try {
    const result = await requestAdmin('/api/mensajes/admin/purge', { method: 'DELETE' });
    renderMessages([]);
    setStatus(`Borrado completado. Filas eliminadas: ${result?.deleted ?? 0}`);
  } catch (error) {
    setStatus(`No se pudo borrar toda la BBDD: ${error.message}`);
  }
};

purgeButton?.addEventListener('click', purgeAll);
adminForm?.addEventListener('submit', async (event) => {
  event.preventDefault();
  await loadMessages();
});

messagesBody?.addEventListener('click', async (event) => {
  const target = event.target;
  if (!(target instanceof HTMLButtonElement)) {
    return;
  }

  const action = target.dataset.action;
  const id = target.dataset.id;

  if (!action || !id) {
    return;
  }

  if (!requireApiKey()) {
    return;
  }

  if (action === 'read') {
    await markAsRead(id);
    return;
  }

  if (action === 'delete') {
    await softDelete(id);
  }
});