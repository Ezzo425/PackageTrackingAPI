const API_BASE_URL = 'http://13.53.67.121:5001/api/packages';
const DEFAULT_API_BASE = API_BASE_URL;
const STORAGE_KEY = 'packageTrackerApiBase';

const STATUS_OPTIONS = [
    { value: 'Created', label: 'Created' },
    { value: 'InTransit', label: 'In transit' },
    { value: 'Delivered', label: 'Delivered' },
    { value: 'Cancelled', label: 'Cancelled' },
];

function getApiBase() {
    const input = document.getElementById('apiBaseUrl');
    const raw = (input?.value || DEFAULT_API_BASE).trim();
    const base = raw.replace(/\/$/, '');
    try {
        localStorage.setItem(STORAGE_KEY, base);
    } catch {
        /* ignore */
    }
    return base;
}

function escapeHtml(value) {
    if (value === null || value === undefined) return '';
    const div = document.createElement('div');
    div.textContent = String(value);
    return div.innerHTML;
}

function formatDateTime(iso) {
    if (!iso) return '—';
    const d = new Date(iso);
    return Number.isNaN(d.getTime()) ? '—' : d.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
}

function toDatetimeLocalValue(iso) {
    if (!iso) return '';
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return '';
    const pad = (n) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function optionalIsoFromDatetimeLocal(value) {
    const v = (value || '').trim();
    if (!v) return null;
    const d = new Date(v);
    return Number.isNaN(d.getTime()) ? null : d.toISOString();
}

function statusBadgeClass(status) {
    switch (status) {
        case 'Delivered':
            return 'success';
        case 'InTransit':
            return 'primary';
        case 'Cancelled':
            return 'danger';
        case 'Created':
        default:
            return 'secondary';
    }
}

function statusLabel(status) {
    const found = STATUS_OPTIONS.find((s) => s.value === status);
    if (found) return found.label;
    if (status == null || status === '') return '—';
    return String(status);
}

async function readErrorMessage(response) {
    try {
        const data = await response.json();
        if (Array.isArray(data)) {
            return data
                .map((e) => {
                    const field = e.field != null ? String(e.field) : '';
                    const errs = Array.isArray(e.errors) ? e.errors.join(', ') : String(e.errors || '');
                    return field ? `${field}: ${errs}` : errs;
                })
                .filter(Boolean)
                .join('; ');
        }
        if (data && typeof data === 'object' && data.errors && typeof data.errors === 'object') {
            return Object.entries(data.errors)
                .map(([k, v]) => `${k}: ${Array.isArray(v) ? v.join(', ') : v}`)
                .join('; ');
        }
        if (data && typeof data.message === 'string') return data.message;
        if (data && typeof data.title === 'string') return data.title;
    } catch {
        /* use status below */
    }
    return response.statusText || `Error ${response.status}`;
}

function alertMarkup(kind, message) {
    const safe = escapeHtml(message);
    return `<div class="alert alert-${kind} mb-0" role="alert">${safe}</div>`;
}

function packageDetailCard(pkg, title) {
    if (!pkg || typeof pkg !== 'object') {
        return alertMarkup('warning', 'No package data returned.');
    }
    const id = pkg.id != null ? pkg.id : '—';
    const tracking = pkg.trackingNumber != null ? pkg.trackingNumber : '—';
    const sender = pkg.senderName != null ? pkg.senderName : '—';
    const receiver = pkg.receiverName != null ? pkg.receiverName : '—';
    const location = pkg.currentLocation != null ? pkg.currentLocation : '—';
    const status = pkg.status != null ? pkg.status : '—';
    const created = formatDateTime(pkg.createdAt);
    const eta = formatDateTime(pkg.estimatedDeliveryDate);
    const t = title ? `<p class="small text-muted mb-2">${escapeHtml(title)}</p>` : '';

    return `
        <div class="card border-info">
            <div class="card-body">
                ${t}
                <ul class="list-unstyled small mb-0">
                    <li><strong>ID:</strong> ${escapeHtml(String(id))}</li>
                    <li><strong>Tracking:</strong> ${escapeHtml(String(tracking))}</li>
                    <li><strong>Sender:</strong> ${escapeHtml(String(sender))}</li>
                    <li><strong>Receiver:</strong> ${escapeHtml(String(receiver))}</li>
                    <li><strong>Status:</strong> <span class="badge bg-${statusBadgeClass(status)}">${escapeHtml(statusLabel(status))}</span></li>
                    <li><strong>Location:</strong> ${escapeHtml(String(location))}</li>
                    <li><strong>Created:</strong> ${escapeHtml(created)}</li>
                    <li><strong>Est. delivery:</strong> ${escapeHtml(eta)}</li>
                </ul>
            </div>
        </div>`;
}

async function trackPackage() {
    const trackingNumber = document.getElementById('trackNumber')?.value?.trim() ?? '';
    const resultDiv = document.getElementById('trackResult');
    if (!resultDiv) return;

    if (!trackingNumber) {
        resultDiv.innerHTML = alertMarkup('warning', 'Enter a tracking number.');
        return;
    }

    resultDiv.innerHTML = alertMarkup('secondary', 'Searching…');
    const base = getApiBase();

    try {
        const response = await fetch(`${base}/tracking/${encodeURIComponent(trackingNumber)}`);
        if (response.status === 404) {
            resultDiv.innerHTML = alertMarkup('danger', 'No package found for that tracking number.');
            return;
        }
        if (response.status === 400) {
            resultDiv.innerHTML = alertMarkup('warning', await readErrorMessage(response));
            return;
        }
        if (!response.ok) {
            resultDiv.innerHTML = alertMarkup('danger', await readErrorMessage(response));
            return;
        }
        const data = await response.json();
        resultDiv.innerHTML = packageDetailCard(data);
    } catch {
        resultDiv.innerHTML = alertMarkup('danger', 'Request failed. Check the API URL and that the server is running.');
    }
}

async function lookupById() {
    const raw = document.getElementById('lookupId')?.value?.trim() ?? '';
    const resultDiv = document.getElementById('lookupResult');
    if (!resultDiv) return;

    const id = parseInt(raw, 10);
    if (!Number.isFinite(id) || id < 1) {
        resultDiv.innerHTML = alertMarkup('warning', 'Enter a valid numeric package ID.');
        return;
    }

    resultDiv.innerHTML = alertMarkup('secondary', 'Loading…');
    const base = getApiBase();

    try {
        const response = await fetch(`${base}/${id}`);
        if (response.status === 404) {
            resultDiv.innerHTML = alertMarkup('danger', 'No package with that ID.');
            return;
        }
        if (!response.ok) {
            resultDiv.innerHTML = alertMarkup('danger', await readErrorMessage(response));
            return;
        }
        const data = await response.json();
        resultDiv.innerHTML = packageDetailCard(data);
    } catch {
        resultDiv.innerHTML = alertMarkup('danger', 'Request failed. Check the API URL and that the server is running.');
    }
}

async function createPackage(ev) {
    ev.preventDefault();
    const msg = document.getElementById('createMessage');
    if (!msg) return;

    const sender = document.getElementById('createSender')?.value?.trim() ?? '';
    const receiver = document.getElementById('createReceiver')?.value?.trim() ?? '';
    const location = document.getElementById('createLocation')?.value?.trim() ?? '';
    const eta = optionalIsoFromDatetimeLocal(document.getElementById('createEta')?.value);

    if (!sender || !receiver || !location) {
        msg.innerHTML = alertMarkup('warning', 'Sender, receiver, and location are required.');
        return;
    }

    msg.innerHTML = alertMarkup('secondary', 'Creating…');
    const base = getApiBase();
    const body = {
        senderName: sender,
        receiverName: receiver,
        currentLocation: location,
        estimatedDeliveryDate: eta,
    };

    try {
        const response = await fetch(base, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
            body: JSON.stringify(body),
        });
        if (!response.ok) {
            msg.innerHTML = alertMarkup('danger', await readErrorMessage(response));
            return;
        }
        const created = await response.json();
        document.getElementById('createForm')?.reset();
        msg.innerHTML = `${alertMarkup('success', 'Package created.')} ${packageDetailCard(created, 'New package')}`;
        await loadAllPackages();
    } catch {
        msg.innerHTML = alertMarkup('danger', 'Request failed. Check the API URL and that the server is running.');
    }
}

let editModalInstance;

function fillStatusSelect(selectEl, selectedValue) {
    if (!selectEl) return;
    selectEl.innerHTML = STATUS_OPTIONS.map(
        (o) => `<option value="${escapeHtml(o.value)}">${escapeHtml(o.label)}</option>`
    ).join('');
    const values = STATUS_OPTIONS.map((o) => o.value);
    const next = values.includes(selectedValue) ? selectedValue : 'Created';
    selectEl.value = next;
}

function openEditModal(pkg) {
    if (!pkg || pkg.id == null) return;
    const idEl = document.getElementById('editId');
    const trackingEl = document.getElementById('editTrackingDisplay');
    if (idEl) idEl.value = String(pkg.id);
    if (trackingEl) trackingEl.textContent = pkg.trackingNumber != null ? String(pkg.trackingNumber) : '—';

    const s = document.getElementById('editSender');
    const r = document.getElementById('editReceiver');
    const l = document.getElementById('editLocation');
    const st = document.getElementById('editStatus');
    const eta = document.getElementById('editEta');
    if (s) s.value = pkg.senderName != null ? String(pkg.senderName) : '';
    if (r) r.value = pkg.receiverName != null ? String(pkg.receiverName) : '';
    if (l) l.value = pkg.currentLocation != null ? String(pkg.currentLocation) : '';
    fillStatusSelect(st, pkg.status != null ? String(pkg.status) : 'Created');
    if (eta) eta.value = toDatetimeLocalValue(pkg.estimatedDeliveryDate);

    editModalInstance?.show();
}

async function submitEdit(ev) {
    ev.preventDefault();
    const idRaw = document.getElementById('editId')?.value;
    const id = parseInt(idRaw, 10);
    if (!Number.isFinite(id)) return;

    const sender = document.getElementById('editSender')?.value?.trim() ?? '';
    const receiver = document.getElementById('editReceiver')?.value?.trim() ?? '';
    const location = document.getElementById('editLocation')?.value?.trim() ?? '';
    const status = document.getElementById('editStatus')?.value;
    const eta = optionalIsoFromDatetimeLocal(document.getElementById('editEta')?.value);

    if (!sender || !receiver || !location || !status) {
        return;
    }

    const base = getApiBase();
    const body = {
        senderName: sender,
        receiverName: receiver,
        currentLocation: location,
        status,
        estimatedDeliveryDate: eta,
    };

    try {
        const response = await fetch(`${base}/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
            body: JSON.stringify(body),
        });
        if (response.status === 404) {
            alert('Package not found. It may have been deleted.');
            return;
        }
        if (!response.ok) {
            alert(await readErrorMessage(response));
            return;
        }
        editModalInstance?.hide();
        await loadAllPackages();
    } catch {
        alert('Update failed. Check the API URL and that the server is running.');
    }
}

async function deletePackage(id) {
    const ok = window.confirm(`Delete package ID ${id}? This cannot be undone.`);
    if (!ok) return;
    const base = getApiBase();
    try {
        const response = await fetch(`${base}/${id}`, { method: 'DELETE' });
        if (response.status === 404) {
            alert('Package not found.');
            return;
        }
        if (!response.ok) {
            alert(await readErrorMessage(response));
            return;
        }
        await loadAllPackages();
    } catch {
        alert('Delete failed. Check the API URL and that the server is running.');
    }
}

function renderPackagesTable(rows) {
    const tableBody = document.getElementById('packagesTableBody');
    if (!tableBody) return;

    if (!Array.isArray(rows) || rows.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="9" class="text-center text-muted py-4">No packages yet. Create one above.</td></tr>`;
        return;
    }

    const frag = document.createDocumentFragment();
    for (const pkg of rows) {
        const tr = document.createElement('tr');
        const id = pkg.id != null ? pkg.id : '';
        const status = pkg.status != null ? pkg.status : '';
        const trId = id !== '' ? `pkg-row-${id}` : '';
        if (trId) tr.id = trId;

        tr.innerHTML = `
            <td>${escapeHtml(id === '' ? '—' : String(id))}</td>
            <td><strong>${escapeHtml(pkg.trackingNumber != null ? String(pkg.trackingNumber) : '—')}</strong></td>
            <td>${escapeHtml(pkg.senderName != null ? String(pkg.senderName) : '—')}</td>
            <td>${escapeHtml(pkg.receiverName != null ? String(pkg.receiverName) : '—')}</td>
            <td><span class="badge bg-${statusBadgeClass(status)}">${escapeHtml(statusLabel(status))}</span></td>
            <td>${escapeHtml(pkg.currentLocation != null ? String(pkg.currentLocation) : '—')}</td>
            <td>${escapeHtml(formatDateTime(pkg.createdAt))}</td>
            <td>${escapeHtml(formatDateTime(pkg.estimatedDeliveryDate))}</td>
            <td class="text-end text-nowrap">
                <button type="button" class="btn btn-sm btn-outline-primary btn-edit" data-id="${escapeHtml(String(id))}">Edit</button>
                <button type="button" class="btn btn-sm btn-outline-danger btn-delete" data-id="${escapeHtml(String(id))}">Delete</button>
            </td>`;
        frag.appendChild(tr);
    }
    tableBody.innerHTML = '';
    tableBody.appendChild(frag);

    tableBody.querySelectorAll('.btn-edit').forEach((btn) => {
        btn.addEventListener('click', () => {
            const pid = parseInt(btn.getAttribute('data-id'), 10);
            const pkg = rows.find((p) => p.id === pid);
            if (pkg) openEditModal(pkg);
        });
    });
    tableBody.querySelectorAll('.btn-delete').forEach((btn) => {
        btn.addEventListener('click', () => {
            const pid = parseInt(btn.getAttribute('data-id'), 10);
            if (Number.isFinite(pid)) deletePackage(pid);
        });
    });
}

async function loadAllPackages() {
    const tableBody = document.getElementById('packagesTableBody');
    if (!tableBody) return;

    tableBody.innerHTML = `<tr><td colspan="9" class="text-center text-muted py-4">Loading…</td></tr>`;
    const base = getApiBase();

    try {
        const response = await fetch(base, { headers: { Accept: 'application/json' } });
        if (!response.ok) {
            tableBody.innerHTML = `<tr><td colspan="9" class="text-center text-danger py-4">${escapeHtml(await readErrorMessage(response))}</td></tr>`;
            return;
        }
        const data = await response.json();
        if (!Array.isArray(data)) {
            tableBody.innerHTML = `<tr><td colspan="9" class="text-center text-warning py-4">Unexpected response from server.</td></tr>`;
            return;
        }
        renderPackagesTable(data);
    } catch {
        tableBody.innerHTML = `<tr><td colspan="9" class="text-center text-danger py-4">${escapeHtml('Could not connect to the API. Check the base URL (production: http://13.53.67.121:5001/api/packages) and that the API is running.')}</td></tr>`;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const apiInput = document.getElementById('apiBaseUrl');
    if (apiInput) {
        try {
            const stored = localStorage.getItem(STORAGE_KEY);
            if (stored) apiInput.value = stored;
            else apiInput.value = DEFAULT_API_BASE;
        } catch {
            apiInput.value = DEFAULT_API_BASE;
        }
        apiInput.addEventListener('change', getApiBase);
    }

    const editModalEl = document.getElementById('editModal');
    if (editModalEl && window.bootstrap?.Modal) {
        editModalInstance = new bootstrap.Modal(editModalEl);
    }

    document.getElementById('btnTrack')?.addEventListener('click', trackPackage);
    document.getElementById('trackNumber')?.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            trackPackage();
        }
    });

    document.getElementById('btnLookupId')?.addEventListener('click', lookupById);
    document.getElementById('lookupId')?.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            lookupById();
        }
    });

    document.getElementById('createForm')?.addEventListener('submit', createPackage);
    document.getElementById('btnRefresh')?.addEventListener('click', loadAllPackages);
    document.getElementById('editForm')?.addEventListener('submit', submitEdit);

    loadAllPackages();
});
