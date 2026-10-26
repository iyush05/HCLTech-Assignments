(function () {
  'use strict';

  // Navigation & tabs
  const tabDirectory = document.getElementById('tab-directory');
  const tabApiExplorer = document.getElementById('tab-api-explorer');
  const viewDirectory = document.getElementById('view-directory-container');
  const viewApi = document.getElementById('view-api-container');

  const statusEl = document.getElementById('status');

  // Directory elements
  const contactsList = document.getElementById('contacts-list');
  const emptyContacts = document.getElementById('empty-contacts');
  const searchInput = document.getElementById('search-input');
  const categoryFilters = document.querySelectorAll('.filters button[data-cat]');
  const toggleFavFilter = document.getElementById('toggle-favorites-filter');
  const prevPageBtn = document.getElementById('prev-page-btn');
  const nextPageBtn = document.getElementById('next-page-btn');
  const paginationInfo = document.getElementById('pagination-info');
  const refreshBtn = document.getElementById('refresh-contacts-btn');
  const exportJsonBtn = document.getElementById('export-json-btn');

  // Stats elements
  const statTotal = document.getElementById('stat-total');
  const statFavs = document.getElementById('stat-favs');
  const statWork = document.getElementById('stat-work');
  const statPersonal = document.getElementById('stat-personal');

  // Form elements
  const formEl = document.getElementById('contact-form');
  const formHeading = document.getElementById('form-heading');
  const contactIdInput = document.getElementById('contact-id');
  const firstNameInput = document.getElementById('contact-firstname');
  const lastNameInput = document.getElementById('contact-lastname');
  const emailInput = document.getElementById('contact-email');
  const phoneInput = document.getElementById('contact-phone');
  const companyInput = document.getElementById('contact-company');
  const jobTitleInput = document.getElementById('contact-jobtitle');
  const categorySelect = document.getElementById('contact-category');
  const favoriteCheckbox = document.getElementById('contact-favorite');
  const notesInput = document.getElementById('contact-notes');
  const saveBtn = document.getElementById('save-contact-btn');
  const cancelBtn = document.getElementById('cancel-contact-btn');

  // API Explorer elements
  const apiPresetSelect = document.getElementById('api-preset-select');
  const httpMethodBadge = document.getElementById('http-method-badge');
  const apiUrlInput = document.getElementById('api-url-input');
  const apiBodyField = document.getElementById('api-body-field');
  const apiBodyInput = document.getElementById('api-body-input');
  const apiSendBtn = document.getElementById('api-send-btn');
  const responseStatusBadge = document.getElementById('response-status-badge');
  const responseTime = document.getElementById('response-time');
  const responseBody = document.getElementById('response-body');

  let currentPage = 1;
  const pageSize = 6;
  let currentCategory = 'all';
  let onlyFavorites = false;
  let searchDebounce = null;
  let sampleContactId = '';

  function setStatus(text, isError = false) {
    statusEl.textContent = text;
    statusEl.className = 'status status-bar' + (isError ? ' error' : '');
  }

  function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }

  // --- TAB SWITCHING ---
  tabDirectory.addEventListener('click', () => {
    tabDirectory.classList.add('active');
    tabDirectory.setAttribute('aria-selected', 'true');
    tabApiExplorer.classList.remove('active');
    tabApiExplorer.setAttribute('aria-selected', 'false');
    viewDirectory.hidden = false;
    viewApi.hidden = true;
  });

  tabApiExplorer.addEventListener('click', () => {
    tabApiExplorer.classList.add('active');
    tabApiExplorer.setAttribute('aria-selected', 'true');
    tabDirectory.classList.remove('active');
    tabDirectory.setAttribute('aria-selected', 'false');
    viewApi.hidden = false;
    viewDirectory.hidden = true;
  });

  // --- DIRECTORY LOGIC ---
  async function loadContacts(page = currentPage) {
    currentPage = page;
    setStatus('Loading directory…');

    const params = new URLSearchParams({
      page: currentPage,
      pageSize: pageSize
    });
    if (currentCategory !== 'all') params.set('category', currentCategory);
    if (onlyFavorites) params.set('favorite', 'true');
    if (searchInput.value.trim()) params.set('search', searchInput.value.trim());

    try {
      const res = await fetch(`/api/contacts?${params.toString()}`);
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      renderContacts(data);
      loadStats();
      setStatus(`Showing page ${data.page} of ${data.totalPages} (${data.totalCount} contact${data.totalCount === 1 ? '' : 's'}).`);
    } catch (err) {
      console.error(err);
      setStatus('Could not connect to contacts service.', true);
    }
  }

  async function loadStats() {
    try {
      const res = await fetch('/api/contacts/stats');
      if (!res.ok) return;
      const stats = await res.json();
      statTotal.textContent = stats.total ?? 0;
      statFavs.textContent = stats.favorites ?? 0;
      statWork.textContent = stats.workCount ?? 0;
      statPersonal.textContent = stats.personalCount ?? 0;
    } catch (err) {
      console.error(err);
    }
  }

  function renderContacts(pagedData) {
    contactsList.innerHTML = '';
    const items = pagedData.items || [];

    if (items.length > 0) {
      sampleContactId = items[0].id;
    }

    if (!items.length) {
      emptyContacts.hidden = false;
      paginationBarUpdate(pagedData);
      return;
    }
    emptyContacts.hidden = true;

    items.forEach(c => {
      const li = document.createElement('li');
      li.className = 'contact-item';

      const initials = ((c.firstName?.[0] || '') + (c.lastName?.[0] || '')).toUpperCase() || 'C';
      const catClass = (c.category || 'personal').toLowerCase();
      const jobLine = [c.jobTitle, c.company].filter(Boolean).join(' at ');

      li.innerHTML = `
        <div class="contact-main">
          <div class="contact-avatar ${escapeHtml(catClass)}">${escapeHtml(initials)}</div>
          <div class="contact-details">
            <div class="contact-name-row">
              <h3 class="contact-name">${escapeHtml(c.firstName)} ${escapeHtml(c.lastName)}</h3>
              <span class="category-badge">${escapeHtml(c.category)}</span>
            </div>
            ${jobLine ? `<div style="font-size: 12px; color: var(--navy); margin-top: 2px;">${escapeHtml(jobLine)}</div>` : ''}
            <div class="contact-meta">
              <span>📧 <a href="mailto:${escapeHtml(c.email)}">${escapeHtml(c.email)}</a></span>
              <span>📞 <a href="tel:${escapeHtml(c.phoneNumber)}">${escapeHtml(c.phoneNumber)}</a></span>
            </div>
          </div>
        </div>
        <div class="contact-actions">
          <button type="button" class="contact-star-btn ${c.isFavorite ? 'active' : ''}" title="${c.isFavorite ? 'Unfavorite' : 'Add to favorites'}" aria-label="Favorite toggle">
            ${c.isFavorite ? '★' : '☆'}
          </button>
          <button type="button" class="action-btn edit-btn">Edit</button>
          <button type="button" class="action-btn danger delete-btn">Delete</button>
        </div>
      `;

      li.querySelector('.contact-star-btn').addEventListener('click', () => toggleFavorite(c.id));
      li.querySelector('.edit-btn').addEventListener('click', () => startEditing(c));
      li.querySelector('.delete-btn').addEventListener('click', () => deleteContact(c.id, `${c.firstName} ${c.lastName}`));

      contactsList.appendChild(li);
    });

    paginationBarUpdate(pagedData);
  }

  function paginationBarUpdate(data) {
    paginationInfo.textContent = `Page ${data.page} of ${data.totalPages || 1} (${data.totalCount || 0} total)`;
    prevPageBtn.disabled = !data.hasPrevious;
    nextPageBtn.disabled = !data.hasNext;
  }

  async function toggleFavorite(id) {
    try {
      const res = await fetch(`/api/contacts/${id}/favorite`, { method: 'PATCH' });
      if (!res.ok) throw new Error('Toggle failed');
      setStatus('Favorite updated.');
      loadContacts();
    } catch {
      setStatus('Could not update favorite.', true);
    }
  }

  async function deleteContact(id, name) {
    if (!confirm(`Delete contact "${name}"?`)) return;
    try {
      const res = await fetch(`/api/contacts/${id}`, { method: 'DELETE' });
      if (!res.ok) throw new Error('Delete failed');
      setStatus(`Contact "${name}" deleted.`);
      if (contactIdInput.value === id) cancelEditing();
      loadContacts();
    } catch {
      setStatus('Could not delete contact.', true);
    }
  }

  function startEditing(c) {
    formHeading.textContent = 'Edit contact';
    contactIdInput.value = c.id;
    firstNameInput.value = c.firstName;
    lastNameInput.value = c.lastName;
    emailInput.value = c.email;
    phoneInput.value = c.phoneNumber;
    companyInput.value = c.company || '';
    jobTitleInput.value = c.jobTitle || '';
    categorySelect.value = c.category || 'Personal';
    favoriteCheckbox.checked = !!c.isFavorite;
    notesInput.value = c.notes || '';
    saveBtn.textContent = 'Save changes';
    cancelBtn.hidden = false;
    firstNameInput.focus();
    window.scrollTo({ top: formEl.offsetTop - 80, behavior: 'smooth' });
  }

  function cancelEditing() {
    formHeading.textContent = 'New contact';
    formEl.reset();
    contactIdInput.value = '';
    saveBtn.textContent = 'Save contact +';
    cancelBtn.hidden = true;
  }

  formEl.addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = contactIdInput.value;
    const payload = {
      firstName: firstNameInput.value.trim(),
      lastName: lastNameInput.value.trim(),
      email: emailInput.value.trim(),
      phoneNumber: phoneInput.value.trim(),
      company: companyInput.value.trim() || null,
      jobTitle: jobTitleInput.value.trim() || null,
      category: categorySelect.value,
      isFavorite: favoriteCheckbox.checked,
      notes: notesInput.value.trim() || null
    };

    saveBtn.disabled = true;
    setStatus('Saving contact…');

    try {
      const url = id ? `/api/contacts/${id}` : '/api/contacts';
      const method = id ? 'PUT' : 'POST';
      const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (!res.ok) {
        const errJson = await res.json().catch(() => ({}));
        throw new Error(errJson.error || `HTTP ${res.status}`);
      }

      setStatus(id ? 'Contact details updated.' : 'New contact created.');
      cancelEditing();
      loadContacts();
    } catch (err) {
      setStatus(err.message || 'Error saving contact.', true);
    } finally {
      saveBtn.disabled = false;
    }
  });

  cancelBtn.addEventListener('click', cancelEditing);

  // Search & Filters
  searchInput.addEventListener('input', () => {
    clearTimeout(searchDebounce);
    searchDebounce = setTimeout(() => loadContacts(1), 250);
  });

  categoryFilters.forEach(btn => {
    btn.addEventListener('click', () => {
      categoryFilters.forEach(b => b.classList.remove('selected'));
      btn.classList.add('selected');
      currentCategory = btn.dataset.cat;
      loadContacts(1);
    });
  });

  toggleFavFilter.addEventListener('click', () => {
    onlyFavorites = !onlyFavorites;
    toggleFavFilter.classList.toggle('selected', onlyFavorites);
    loadContacts(1);
  });

  prevPageBtn.addEventListener('click', () => {
    if (currentPage > 1) loadContacts(currentPage - 1);
  });
  nextPageBtn.addEventListener('click', () => {
    loadContacts(currentPage + 1);
  });

  refreshBtn.addEventListener('click', () => loadContacts(currentPage));

  exportJsonBtn.addEventListener('click', () => {
    window.location.href = '/api/contacts/export';
  });

  // --- INTERACTIVE API EXPLORER LOGIC ---
  const templates = {
    'get-all': {
      method: 'GET',
      url: '/api/contacts?page=1&pageSize=10',
      hasBody: false,
      body: ''
    },
    'get-single': {
      method: 'GET',
      url: () => `/api/contacts/${sampleContactId || '00000000-0000-0000-0000-000000000000'}`,
      hasBody: false,
      body: ''
    },
    'post-create': {
      method: 'POST',
      url: '/api/contacts',
      hasBody: true,
      body: JSON.stringify({
        firstName: "Claude",
        lastName: "Shannon",
        email: "shannon@bell.labs",
        phoneNumber: "+1 908 555 0123",
        company: "Bell Telephone Labs",
        jobTitle: "Father of Information Theory",
        category: "Work",
        isFavorite: true,
        notes: "Founded digital circuit design theory."
      }, null, 2)
    },
    'put-update': {
      method: 'PUT',
      url: () => `/api/contacts/${sampleContactId || '00000000-0000-0000-0000-000000000000'}`,
      hasBody: true,
      body: JSON.stringify({
        firstName: "Ada",
        lastName: "Lovelace",
        email: "ada@computing.org",
        phoneNumber: "+44 20 7946 0910",
        company: "Analytical Engine Works",
        jobTitle: "Chief Programmer",
        category: "Work",
        isFavorite: true,
        notes: "Updated record."
      }, null, 2)
    },
    'delete-single': {
      method: 'DELETE',
      url: () => `/api/contacts/${sampleContactId || '00000000-0000-0000-0000-000000000000'}`,
      hasBody: false,
      body: ''
    },
    'patch-fav': {
      method: 'PATCH',
      url: () => `/api/contacts/${sampleContactId || '00000000-0000-0000-0000-000000000000'}/favorite`,
      hasBody: false,
      body: ''
    },
    'get-stats': {
      method: 'GET',
      url: '/api/contacts/stats',
      hasBody: false,
      body: ''
    }
  };

  function applyPreset(key) {
    const t = templates[key];
    if (!t) return;

    const method = t.method;
    httpMethodBadge.textContent = method;
    httpMethodBadge.className = `http-badge ${method.toLowerCase()}`;

    apiUrlInput.value = typeof t.url === 'function' ? t.url() : t.url;
    apiBodyField.hidden = !t.hasBody;
    if (t.hasBody) {
      apiBodyInput.value = t.body;
    }
  }

  apiPresetSelect.addEventListener('change', () => applyPreset(apiPresetSelect.value));

  apiSendBtn.addEventListener('click', async () => {
    const method = httpMethodBadge.textContent.trim();
    const url = apiUrlInput.value.trim();
    const options = {
      method: method,
      headers: {}
    };

    if (method === 'POST' || method === 'PUT') {
      options.headers['Content-Type'] = 'application/json';
      options.body = apiBodyInput.value;
    }

    responseStatusBadge.textContent = 'Sending…';
    responseStatusBadge.className = 'status-pill status-ready';
    responseTime.textContent = '…';
    responseBody.textContent = 'Awaiting HTTP response…';

    const startTime = performance.now();
    try {
      const res = await fetch(url, options);
      const elapsed = Math.round(performance.now() - startTime);
      responseTime.textContent = `${elapsed} ms`;

      responseStatusBadge.textContent = `${res.status} ${res.statusText || ''}`;
      const statusClass = `status-${res.status}`;
      responseStatusBadge.className = `status-pill ${statusClass}`;

      const contentType = res.headers.get('content-type') || '';
      let text = await res.text();

      if (contentType.includes('application/json') && text.trim()) {
        try {
          const parsed = JSON.parse(text);
          responseBody.textContent = JSON.stringify(parsed, null, 2);
        } catch {
          responseBody.textContent = text;
        }
      } else if (!text.trim()) {
        responseBody.textContent = `(HTTP ${res.status}: Empty response body)`;
      } else {
        responseBody.textContent = text;
      }

      // Also reload contacts list if a mutating operation was successful
      if (['POST', 'PUT', 'DELETE', 'PATCH'].includes(method) && res.ok) {
        loadContacts();
      }
    } catch (err) {
      const elapsed = Math.round(performance.now() - startTime);
      responseTime.textContent = `${elapsed} ms`;
      responseStatusBadge.textContent = 'Network Error';
      responseStatusBadge.className = 'status-pill status-400';
      responseBody.textContent = `Fetch error: ${err.message}`;
    }
  });

  // Startup
  loadContacts();
  applyPreset('get-all');
})();
