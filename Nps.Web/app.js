'use strict';

// Адрес API користувача з Render. Не додавайте сюди пароль чи connection string.
const API_BASE = 'https://nps-system-lab7.onrender.com';
const DEVICES_URL = `${API_BASE}/api/Devices`;

const $ = (id) => document.getElementById(id);
let devices = [];
let isLoading = false;

function escapeHtml(value) {
  return String(value ?? '').replace(/[&<>"']/g, (char) => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
  })[char]);
}

function showToast(message, type = 'success') {
  const node = document.createElement('div');
  node.className = `toast ${type}`;
  node.textContent = message;
  $('toastRegion').appendChild(node);
  window.setTimeout(() => node.remove(), 4200);
}

async function request(url, options = {}) {
  let response;
  try {
    response = await fetch(url, {
      ...options,
      headers: {
        ...(options.body ? { 'Content-Type': 'application/json' } : {}),
        ...(options.headers || {})
      }
    });
  } catch (error) {
    if (error instanceof TypeError) {
      throw new Error('Не вдалося звернутися до API. Перевірте доступність сервісу та налаштування CORS для адреси вебклієнта.');
    }
    throw error;
  }

  const text = await response.text();
  let data = null;
  if (text) {
    try { data = JSON.parse(text); } catch { data = text; }
  }
  if (!response.ok) {
    const detail = data && typeof data === 'object'
      ? (data.detail || data.title || Object.values(data.errors || {}).flat().join(' '))
      : data;
    throw new Error(`${response.status}: ${detail || response.statusText || 'Помилка запиту'}`);
  }
  return data;
}

function normalizeDevice(device) {
  return {
    id: device.id ?? device.Id,
    name: device.name ?? device.Name ?? 'Без назви',
    type: device.type ?? device.Type ?? 'Sensor',
    isEnabled: device.isEnabled ?? device.IsEnabled ?? false,
    temperature: device.temperature ?? device.Temperature ?? null,
    lastUpdated: device.lastUpdated ?? device.LastUpdated ?? null
  };
}

function formatDate(value) {
  if (!value) return '—';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '—';
  return new Intl.DateTimeFormat('uk-UA', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  }).format(date);
}

function updateStats(list) {
  $('statTotal').textContent = list.length;
  $('statSensors').textContent = list.filter(x => String(x.type).toLowerCase() === 'sensor').length;
  $('statActuators').textContent = list.filter(x => String(x.type).toLowerCase() === 'actuator').length;
  $('statEnabled').textContent = list.filter(x => Boolean(x.isEnabled)).length;
  $('recordCount').textContent = `${list.length} ${list.length === 1 ? 'запис' : list.length < 5 ? 'записи' : 'записів'}`;
}

function filteredDevices() {
  const query = $('searchInput').value.trim().toLocaleLowerCase('uk-UA');
  const type = $('typeFilter').value;
  return devices.filter(device => {
    const matchesName = device.name.toLocaleLowerCase('uk-UA').includes(query);
    const matchesType = type === 'all' || String(device.type).toLowerCase() === type.toLowerCase();
    return matchesName && matchesType;
  });
}

function renderDevices() {
  const list = filteredDevices();
  updateStats(devices);
  if (!list.length) {
    $('devicesBody').innerHTML = `<tr><td colspan="6" class="table-message">${devices.length ? 'За вибраними фільтрами нічого не знайдено.' : 'Пристроїв поки немає. Додайте перший запис праворуч.'}</td></tr>`;
    return;
  }

  $('devicesBody').innerHTML = list.map(device => {
    const sensor = String(device.type).toLowerCase() === 'sensor';
    const enabled = Boolean(device.isEnabled);
    const temperature = device.temperature === null || device.temperature === undefined
      ? '<span class="temperature missing">Не вказано</span>'
      : `<span class="temperature">${escapeHtml(device.temperature)} °C</span>`;
    return `<tr>
      <td class="device-name" title="${escapeHtml(device.name)}">${escapeHtml(device.name)}</td>
      <td><span class="type-tag ${sensor ? 'sensor' : 'actuator'}">${sensor ? '⌁ Датчик' : '◈ Механізм'}</span></td>
      <td><span class="state-tag ${enabled ? 'enabled' : 'disabled'}">${enabled ? 'Увімкнено' : 'Вимкнено'}</span></td>
      <td>${temperature}</td>
      <td class="date-cell">${escapeHtml(formatDate(device.lastUpdated))}</td>
      <td><div class="actions">
        <button class="action-button" data-action="toggle" data-id="${escapeHtml(device.id)}">${enabled ? 'Вимкнути' : 'Увімкнути'}</button>
        <button class="action-button" data-action="edit" data-id="${escapeHtml(device.id)}">Змінити</button>
        <button class="action-button danger" data-action="delete" data-id="${escapeHtml(device.id)}">Видалити</button>
      </div></td>
    </tr>`;
  }).join('');
}

async function loadDevices(showMessage = false) {
  if (isLoading) return;
  isLoading = true;
  $('refreshButton').disabled = true;
  $('devicesBody').innerHTML = '<tr><td colspan="6" class="table-message">Завантаження даних з API…</td></tr>';
  try {
    const data = await request(DEVICES_URL, { method: 'GET' });
    if (!Array.isArray(data)) throw new Error('API повернуло не список пристроїв. Перевірте відповідь GET /api/Devices.');
    devices = data.map(normalizeDevice);
    renderDevices();
    const now = new Date();
    $('lastRefresh').textContent = new Intl.DateTimeFormat('uk-UA', { hour: '2-digit', minute: '2-digit', second: '2-digit' }).format(now);
    $('tableHint').textContent = `Отримано ${devices.length} записів із PostgreSQL через Nps.API`;
    if (showMessage) showToast('Список пристроїв оновлено.');
  } catch (error) {
    $('devicesBody').innerHTML = `<tr><td colspan="6" class="table-message">${escapeHtml(error.message)}</td></tr>`;
    $('tableHint').textContent = 'Не вдалося отримати дані з API';
    if (showMessage) showToast(error.message, 'error');
  } finally {
    isLoading = false;
    $('refreshButton').disabled = false;
  }
}

async function checkHealth() {
  const badge = $('connectionBadge');
  try {
    const result = await request(`${API_BASE}/health`);
    const healthy = typeof result === 'string' ? result.toLowerCase().includes('healthy') : true;
    if (!healthy) throw new Error('Health check не повернув Healthy');
    badge.classList.remove('offline');
    badge.classList.add('online');
    $('connectionText').textContent = 'API доступний';
    $('apiMiniText').textContent = 'З’єднання з API активне';
    $('apiMiniPulse').classList.add('online');
  } catch (error) {
    badge.classList.remove('online');
    badge.classList.add('offline');
    $('connectionText').textContent = 'API недоступний';
    $('apiMiniText').textContent = 'Перевірте сервіс або CORS';
    $('apiMiniPulse').classList.remove('online');
  }
}

async function createDevice(event) {
  event.preventDefault();
  const button = $('createButton');
  const name = $('deviceName').value.trim();
  const type = $('deviceType').value;
  const temperatureText = $('deviceTemperature').value.trim();
  const temperature = temperatureText === '' ? null : Number(temperatureText);
  if (name.length < 2 || name.length > 100) return showToast('Назва має містити від 2 до 100 символів.', 'error');
  if (temperature !== null && (!Number.isFinite(temperature) || temperature < -50 || temperature > 400)) return showToast('Температура має бути в діапазоні від −50 до 400 °C.', 'error');
  button.disabled = true;
  try {
    await request(DEVICES_URL, {
      method: 'POST',
      body: JSON.stringify({ name, type, temperature })
    });
    $('createForm').reset();
    showToast('Пристрій успішно створено.');
    await loadDevices();
  } catch (error) {
    showToast(error.message, 'error');
  } finally {
    button.disabled = false;
  }
}

async function toggleDevice(id) {
  const device = devices.find(item => String(item.id) === String(id));
  if (!device) return;
  try {
    await request(`${DEVICES_URL}/${encodeURIComponent(id)}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ isActive: !device.isEnabled })
    });
    showToast(`Стан пристрою «${device.name}» оновлено.`);
    await loadDevices();
  } catch (error) {
    showToast(error.message, 'error');
  }
}

function openEditDialog(id) {
  const device = devices.find(item => String(item.id) === String(id));
  if (!device) return;
  $('editId').value = device.id;
  $('editName').value = device.name;
  $('editType').value = device.type;
  $('editTemperature').value = device.temperature ?? '';
  $('editDialog').showModal();
}

async function saveEdit(event) {
  event.preventDefault();
  const id = $('editId').value;
  const name = $('editName').value.trim();
  const type = $('editType').value;
  const temperatureText = $('editTemperature').value.trim();
  const temperature = temperatureText === '' ? null : Number(temperatureText);
  if (name.length < 2 || name.length > 100) return showToast('Назва має містити від 2 до 100 символів.', 'error');
  if (temperature !== null && (!Number.isFinite(temperature) || temperature < -50 || temperature > 400)) return showToast('Температура має бути в діапазоні від −50 до 400 °C.', 'error');
  const button = $('saveEditButton');
  button.disabled = true;
  try {
    await request(`${DEVICES_URL}/${encodeURIComponent(id)}`, {
      method: 'PUT',
      body: JSON.stringify({ name, type, temperature })
    });
    $('editDialog').close();
    showToast('Зміни збережено.');
    await loadDevices();
  } catch (error) {
    showToast(error.message, 'error');
  } finally {
    button.disabled = false;
  }
}

async function deleteDevice(id) {
  const device = devices.find(item => String(item.id) === String(id));
  if (!device) return;
  if (!window.confirm(`Видалити запис «${device.name}»?`)) return;
  try {
    await request(`${DEVICES_URL}/${encodeURIComponent(id)}`, { method: 'DELETE' });
    showToast('Запис видалено.');
    await loadDevices();
  } catch (error) {
    showToast(error.message, 'error');
  }
}

$('createForm').addEventListener('submit', createDevice);
$('editForm').addEventListener('submit', saveEdit);
$('refreshButton').addEventListener('click', () => loadDevices(true));
$('searchInput').addEventListener('input', renderDevices);
$('typeFilter').addEventListener('change', renderDevices);
$('clearFilters').addEventListener('click', () => { $('searchInput').value = ''; $('typeFilter').value = 'all'; renderDevices(); });
$('devicesBody').addEventListener('click', (event) => {
  const button = event.target.closest('button[data-action]');
  if (!button) return;
  const { action, id } = button.dataset;
  if (action === 'toggle') toggleDevice(id);
  if (action === 'edit') openEditDialog(id);
  if (action === 'delete') deleteDevice(id);
});
$('closeDialog').addEventListener('click', () => $('editDialog').close());
$('cancelEdit').addEventListener('click', () => $('editDialog').close());

checkHealth();
loadDevices();
window.setInterval(checkHealth, 60000);
