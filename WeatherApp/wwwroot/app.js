(function () {
  'use strict';

  const statusEl = document.getElementById('status');
  const citySearchInput = document.getElementById('city-search');
  const searchForm = document.getElementById('search-form');
  const searchDropdown = document.getElementById('search-dropdown');
  const unitToggle = document.getElementById('unit-toggle');
  const unitC = document.getElementById('unit-c');
  const unitF = document.getElementById('unit-f');
  const saveFavoriteBtn = document.getElementById('save-favorite-btn');

  // Hero report elements
  const reportCity = document.getElementById('report-city');
  const reportCountry = document.getElementById('report-country');
  const currentTemp = document.getElementById('current-temp');
  const currentUnit = document.getElementById('current-unit');
  const currentIcon = document.getElementById('current-icon');
  const currentCondition = document.getElementById('current-condition');
  const feelsLikeTemp = document.getElementById('feels-like-temp');
  const metricHumidity = document.getElementById('metric-humidity');
  const metricWind = document.getElementById('metric-wind');
  const windUnitLabel = document.getElementById('wind-unit-label');
  const metricPrecip = document.getElementById('metric-precip');
  const dataSourceBadge = document.getElementById('data-source-badge');

  const hourlyStrip = document.getElementById('hourly-strip');
  const dailyForecast = document.getElementById('daily-forecast');

  const favoritesList = document.getElementById('favorites-list');
  const noFavorites = document.getElementById('no-favorites');
  const favCount = document.getElementById('fav-count');
  const recentSearchesEl = document.getElementById('recent-searches');

  let currentUnitVal = 'c';
  let currentLocation = {
    city: 'Tokyo',
    country: 'Japan',
    lat: 35.6762,
    lon: 139.6503
  };
  let searchDebounce = null;

  function setStatus(text, isError = false) {
    statusEl.textContent = text;
    statusEl.className = 'status status-bar' + (isError ? ' error' : '');
  }

  function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }

  async function loadWeather(loc = currentLocation) {
    currentLocation = loc;
    setStatus(`Fetching weather report for ${loc.city}…`);

    const params = new URLSearchParams({
      lat: loc.lat,
      lon: loc.lon,
      city: loc.city,
      country: loc.country || '',
      unit: currentUnitVal
    });

    try {
      const res = await fetch(`/api/weather/report?${params.toString()}`);
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      renderReport(data);
      setStatus(`Updated conditions for ${data.city}.`);
    } catch (err) {
      console.error(err);
      setStatus(`Could not load weather report. (${err.message})`, true);
    }
  }

  function renderReport(report) {
    reportCity.textContent = report.city;
    reportCountry.textContent = report.country ? report.country : 'Coordinates: ' + report.latitude.toFixed(2) + ', ' + report.longitude.toFixed(2);

    const cur = report.current;
    currentTemp.textContent = cur.temperature;
    currentUnit.textContent = report.unit;
    currentIcon.textContent = cur.weatherIcon;
    currentCondition.textContent = cur.weatherDescription;
    feelsLikeTemp.textContent = `${cur.feelsLike}${report.unit}`;

    metricHumidity.textContent = `${cur.humidity}%`;
    const windSpeedUnit = report.unit === '°F' ? 'mph' : 'km/h';
    metricWind.textContent = `${cur.windSpeed} ${windSpeedUnit}`;
    windUnitLabel.textContent = `Wind (${windSpeedUnit})`;

    const precipUnit = report.unit === '°F' ? 'in' : 'mm';
    metricPrecip.textContent = `${cur.precipitation} ${precipUnit}`;

    dataSourceBadge.textContent = report.isOfflineData ? 'Offline simulation' : 'Open-Meteo API';
    dataSourceBadge.style.color = report.isOfflineData ? '#ad622a' : '#236950';

    // Render hourly
    hourlyStrip.innerHTML = '';
    (report.hourly || []).slice(0, 24).forEach(h => {
      const card = document.createElement('div');
      card.className = 'hourly-card';
      card.innerHTML = `
        <div class="hourly-time">${escapeHtml(h.time)}</div>
        <div class="hourly-icon">${escapeHtml(h.weatherIcon)}</div>
        <div class="hourly-temp">${h.temperature}${report.unit}</div>
      `;
      hourlyStrip.appendChild(card);
    });

    // Render daily
    dailyForecast.innerHTML = '';
    (report.daily || []).forEach(d => {
      const row = document.createElement('div');
      row.className = 'daily-row';
      row.innerHTML = `
        <div class="daily-day">${escapeHtml(d.dayName)}</div>
        <div class="daily-icon">${escapeHtml(d.weatherIcon)}</div>
        <div class="daily-desc">${escapeHtml(d.weatherDescription)}</div>
        <div class="daily-range">
          <span class="daily-max">${d.maxTemp}${report.unit}</span>
          <span class="daily-min">${d.minTemp}${report.unit}</span>
        </div>
      `;
      dailyForecast.appendChild(row);
    });
  }

  async function loadFavorites() {
    try {
      const res = await fetch('/api/weather/favorites');
      if (!res.ok) return;
      const favs = await res.json();
      renderFavorites(favs);
    } catch (err) {
      console.error('Failed to load favorites', err);
    }
  }

  function renderFavorites(favs) {
    favoritesList.innerHTML = '';
    favCount.textContent = `${favs.length} ${favs.length === 1 ? 'city' : 'cities'}`;
    if (!favs.length) {
      noFavorites.hidden = false;
      return;
    }
    noFavorites.hidden = true;

    favs.forEach(fav => {
      const li = document.createElement('li');
      li.className = 'favorite-item';
      li.innerHTML = `
        <div class="fav-info">
          <strong>${escapeHtml(fav.city)}</strong>
          <span>${escapeHtml(fav.country || '')}</span>
        </div>
        <button type="button" class="fav-delete-btn" title="Remove favorite" aria-label="Remove favorite">✕</button>
      `;

      li.addEventListener('click', (e) => {
        if (e.target.closest('.fav-delete-btn')) return;
        loadWeather({
          city: fav.city,
          country: fav.country,
          lat: fav.latitude,
          lon: fav.longitude
        });
      });

      li.querySelector('.fav-delete-btn').addEventListener('click', async (e) => {
        e.stopPropagation();
        await removeFavorite(fav.id, fav.city);
      });

      favoritesList.appendChild(li);
    });
  }

  async function removeFavorite(id, name) {
    try {
      const res = await fetch(`/api/weather/favorites/${id}`, { method: 'DELETE' });
      if (!res.ok) throw new Error('Delete failed');
      setStatus(`Removed ${name} from favorites.`);
      loadFavorites();
    } catch {
      setStatus('Could not remove favorite.', true);
    }
  }

  saveFavoriteBtn.addEventListener('click', async () => {
    saveFavoriteBtn.disabled = true;
    try {
      const res = await fetch('/api/weather/favorites', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          city: currentLocation.city,
          country: currentLocation.country,
          latitude: currentLocation.lat,
          longitude: currentLocation.lon
        })
      });
      if (!res.ok) throw new Error('Failed to save');
      setStatus(`Added ${currentLocation.city} to favorites.`);
      loadFavorites();
    } catch {
      setStatus('Could not save favorite.', true);
    } finally {
      saveFavoriteBtn.disabled = false;
    }
  });

  async function loadRecentSearches() {
    try {
      const res = await fetch('/api/weather/history');
      if (!res.ok) return;
      const history = await res.json();
      recentSearchesEl.innerHTML = '';
      if (!history.length) {
        recentSearchesEl.innerHTML = '<span style="font-size:12px;color:var(--muted)">No recent searches.</span>';
        return;
      }
      history.forEach(query => {
        const chip = document.createElement('button');
        chip.type = 'button';
        chip.className = 'search-chip';
        chip.textContent = query;
        chip.addEventListener('click', () => {
          citySearchInput.value = query;
          searchCity(query);
        });
        recentSearchesEl.appendChild(chip);
      });
    } catch (err) {
      console.error(err);
    }
  }

  async function searchCity(query) {
    if (!query.trim()) return;
    setStatus(`Searching for "${query}"…`);
    try {
      const res = await fetch(`/api/weather/search?query=${encodeURIComponent(query.trim())}`);
      if (!res.ok) throw new Error('Search failed');
      const results = await res.json();
      if (results.length > 0) {
        const top = results[0];
        loadWeather({
          city: top.name,
          country: top.country,
          lat: top.latitude,
          lon: top.longitude
        });
        searchDropdown.hidden = true;
        loadRecentSearches();
      } else {
        setStatus(`No location matches found for "${query}".`, true);
      }
    } catch (err) {
      setStatus(`Search failed: ${err.message}`, true);
    }
  }

  // Live autocomplete dropdown
  citySearchInput.addEventListener('input', () => {
    clearTimeout(searchDebounce);
    const q = citySearchInput.value.trim();
    if (q.length < 2) {
      searchDropdown.hidden = true;
      return;
    }

    searchDebounce = setTimeout(async () => {
      try {
        const res = await fetch(`/api/weather/search?query=${encodeURIComponent(q)}`);
        if (!res.ok) return;
        const results = await res.json();
        if (!results.length) {
          searchDropdown.hidden = true;
          return;
        }

        searchDropdown.innerHTML = '';
        results.slice(0, 5).forEach(item => {
          const div = document.createElement('div');
          div.className = 'search-dropdown-item';
          const subtitle = [item.admin1, item.country].filter(Boolean).join(', ');
          div.innerHTML = `<strong>${escapeHtml(item.name)}</strong><span>${escapeHtml(subtitle)}</span>`;
          div.addEventListener('click', () => {
            citySearchInput.value = item.name;
            searchDropdown.hidden = true;
            loadWeather({
              city: item.name,
              country: item.country,
              lat: item.latitude,
              lon: item.longitude
            });
            loadRecentSearches();
          });
          searchDropdown.appendChild(div);
        });
        searchDropdown.hidden = false;
      } catch (err) {
        console.error(err);
      }
    }, 200);
  });

  document.addEventListener('click', (e) => {
    if (!searchForm.contains(e.target)) {
      searchDropdown.hidden = true;
    }
  });

  searchForm.addEventListener('submit', (e) => {
    e.preventDefault();
    searchDropdown.hidden = true;
    searchCity(citySearchInput.value);
  });

  // Popular city chips
  document.querySelectorAll('.city-chip').forEach(btn => {
    btn.addEventListener('click', () => {
      loadWeather({
        city: btn.dataset.city,
        country: btn.dataset.country,
        lat: parseFloat(btn.dataset.lat),
        lon: parseFloat(btn.dataset.lon)
      });
    });
  });

  // Unit toggle
  unitToggle.addEventListener('click', () => {
    currentUnitVal = currentUnitVal === 'c' ? 'f' : 'c';
    unitC.classList.toggle('active', currentUnitVal === 'c');
    unitF.classList.toggle('active', currentUnitVal === 'f');
    loadWeather(currentLocation);
  });

  // Startup
  loadWeather(currentLocation);
  loadFavorites();
  loadRecentSearches();
})();
