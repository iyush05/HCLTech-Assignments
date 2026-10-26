(function () {
  'use strict';

  const statusEl = document.getElementById('status');
  const notesGridEl = document.getElementById('notes-grid');
  const emptyStateEl = document.getElementById('empty-state');
  const searchInput = document.getElementById('search-input');
  const categoryFilter = document.getElementById('category-filter');
  const togglePinnedBtn = document.getElementById('toggle-pinned-only');
  const refreshBtn = document.getElementById('refresh-btn');

  const statTotal = document.getElementById('stat-total');
  const statPinned = document.getElementById('stat-pinned');
  const statWords = document.getElementById('stat-words');

  const formEl = document.getElementById('note-form');
  const formHeading = document.getElementById('form-heading');
  const noteIdInput = document.getElementById('note-id');
  const titleInput = document.getElementById('note-title');
  const categoryInput = document.getElementById('note-category');
  const contentInput = document.getElementById('note-content');
  const contentCount = document.getElementById('content-count');
  const pinnedCheckbox = document.getElementById('note-pinned');
  const saveBtn = document.getElementById('save-btn');
  const cancelBtn = document.getElementById('cancel-btn');

  let currentNotes = [];
  let pinnedOnly = false;
  let searchTimeout = null;

  function setStatus(text, isError = false) {
    statusEl.textContent = text;
    statusEl.className = 'status status-bar' + (isError ? ' error' : '');
  }

  function updateCharCount() {
    const len = contentInput.value.length;
    contentCount.textContent = `${len} / 5000`;
  }

  function getSelectedColor() {
    const checked = formEl.querySelector('input[name="note-color"]:checked');
    return checked ? checked.value : 'mint';
  }

  function setSelectedColor(color) {
    const radio = formEl.querySelector(`input[name="note-color"][value="${color}"]`);
    if (radio) radio.checked = true;
  }

  async function loadNotes() {
    setStatus('Loading notes…');
    const params = new URLSearchParams();
    if (categoryFilter.value && categoryFilter.value !== 'all') {
      params.set('category', categoryFilter.value);
    }
    if (searchInput.value.trim()) {
      params.set('search', searchInput.value.trim());
    }
    if (pinnedOnly) {
      params.set('pinned', 'true');
    }

    try {
      const res = await fetch(`/api/notes?${params.toString()}`);
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      currentNotes = data.notes || [];
      renderNotes(currentNotes);
      renderStats(data.stats);
      updateCategoryDropdown(currentNotes);
      setStatus(currentNotes.length ? `Showing ${currentNotes.length} note${currentNotes.length === 1 ? '' : 's'}.` : 'No matching notes found.');
    } catch (err) {
      console.error(err);
      setStatus('Could not connect to the notes service. Check server status.', true);
    }
  }

  function renderStats(stats) {
    if (!stats) return;
    statTotal.textContent = stats.total ?? 0;
    statPinned.textContent = stats.pinned ?? 0;
    statWords.textContent = (stats.totalWords ?? 0).toLocaleString();
  }

  function updateCategoryDropdown(notes) {
    const set = new Set();
    notes.forEach(n => { if (n.category) set.add(n.category); });
    const currentVal = categoryFilter.value;
    categoryFilter.innerHTML = '<option value="all">All categories</option>';
    Array.from(set).sort().forEach(cat => {
      const opt = document.createElement('option');
      opt.value = cat;
      opt.textContent = cat;
      if (cat === currentVal) opt.selected = true;
      categoryFilter.appendChild(opt);
    });
  }

  function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }

  function formatDate(iso) {
    if (!iso) return '';
    try {
      const d = new Date(iso);
      return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
    } catch {
      return '';
    }
  }

  function renderNotes(notes) {
    notesGridEl.innerHTML = '';
    if (!notes.length) {
      emptyStateEl.hidden = false;
      return;
    }
    emptyStateEl.hidden = true;

    notes.forEach(note => {
      const card = document.createElement('article');
      card.className = `note-card ${note.isPinned ? 'pinned' : ''}`;
      card.dataset.id = note.id;

      const barColor = note.color || 'mint';
      card.innerHTML = `
        <div class="note-card-bar ${escapeHtml(barColor)}"></div>
        <div class="note-header">
          <span class="note-category-badge">${escapeHtml(note.category || 'General')}</span>
          <button class="pin-btn ${note.isPinned ? 'active' : ''}" title="${note.isPinned ? 'Unpin note' : 'Pin to top'}" aria-label="Pin toggle">
            ${note.isPinned ? '📌' : '⚲'}
          </button>
        </div>
        <h3 class="note-title">${escapeHtml(note.title)}</h3>
        <div class="note-body">${escapeHtml(note.content)}</div>
        <div class="note-footer">
          <span>${formatDate(note.updatedAt || note.createdAt)}</span>
          <div class="note-actions">
            <button class="note-action-btn edit-btn">Edit</button>
            <button class="note-action-btn danger delete-btn">Delete</button>
          </div>
        </div>
      `;

      card.querySelector('.pin-btn').addEventListener('click', () => togglePin(note.id));
      card.querySelector('.edit-btn').addEventListener('click', () => startEditing(note));
      card.querySelector('.delete-btn').addEventListener('click', () => deleteNote(note.id, note.title));

      notesGridEl.appendChild(card);
    });
  }

  async function togglePin(id) {
    try {
      const res = await fetch(`/api/notes/${id}/pin`, { method: 'PATCH' });
      if (!res.ok) throw new Error('Pin failed');
      setStatus('Pin updated.');
      loadNotes();
    } catch (err) {
      setStatus('Failed to update pin status.', true);
    }
  }

  async function deleteNote(id, title) {
    if (!confirm(`Delete note "${title}"?`)) return;
    try {
      const res = await fetch(`/api/notes/${id}`, { method: 'DELETE' });
      if (!res.ok) throw new Error('Delete failed');
      setStatus('Note deleted.');
      if (noteIdInput.value === id) cancelEditing();
      loadNotes();
    } catch (err) {
      setStatus('Could not delete note.', true);
    }
  }

  function startEditing(note) {
    formHeading.textContent = 'Edit note';
    noteIdInput.value = note.id;
    titleInput.value = note.title;
    categoryInput.value = note.category;
    contentInput.value = note.content;
    pinnedCheckbox.checked = !!note.isPinned;
    setSelectedColor(note.color || 'mint');
    saveBtn.textContent = 'Save changes';
    cancelBtn.hidden = false;
    updateCharCount();
    titleInput.focus();
    window.scrollTo({ top: formEl.offsetTop - 80, behavior: 'smooth' });
  }

  function cancelEditing() {
    formHeading.textContent = 'Capture a note';
    formEl.reset();
    noteIdInput.value = '';
    setSelectedColor('mint');
    saveBtn.textContent = 'Save note +';
    cancelBtn.hidden = true;
    updateCharCount();
  }

  formEl.addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = noteIdInput.value;
    const payload = {
      title: titleInput.value.trim(),
      category: categoryInput.value.trim() || 'General',
      content: contentInput.value.trim(),
      color: getSelectedColor(),
      isPinned: pinnedCheckbox.checked
    };

    saveBtn.disabled = true;
    setStatus('Saving note…');

    try {
      const url = id ? `/api/notes/${id}` : '/api/notes';
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

      setStatus(id ? 'Note changes saved.' : 'New note created.');
      cancelEditing();
      loadNotes();
    } catch (err) {
      setStatus(err.message || 'Error saving note.', true);
    } finally {
      saveBtn.disabled = false;
    }
  });

  cancelBtn.addEventListener('click', cancelEditing);
  contentInput.addEventListener('input', updateCharCount);

  searchInput.addEventListener('input', () => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(loadNotes, 250);
  });

  categoryFilter.addEventListener('change', loadNotes);
  refreshBtn.addEventListener('click', loadNotes);

  togglePinnedBtn.addEventListener('click', () => {
    pinnedOnly = !pinnedOnly;
    togglePinnedBtn.setAttribute('aria-pressed', String(pinnedOnly));
    togglePinnedBtn.classList.toggle('selected', pinnedOnly);
    loadNotes();
  });

  updateCharCount();
  loadNotes();
})();
