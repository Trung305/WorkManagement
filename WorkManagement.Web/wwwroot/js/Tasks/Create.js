
const dropZone = document.getElementById('dropZone');
const fileInput = document.getElementById('fileInput');
const fileList = document.getElementById('fileList');
let selectedFiles = [];
flatpickr("#startDate-picker", {
    enableTime: true,
    dateFormat: "d/m/Y H:i",
    time_24hr: true,
    locale: "vn",
    minuteIncrement: 15,
    onChange: function (selectedDates, dateStr) {
        if (!selectedDates[0]) return;
        const d = selectedDates[0];
        const val = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}T${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
        document.getElementById('startDate-hidden').value = val;
    }
});

flatpickr("#deadline-picker", {
    enableTime: true,
    dateFormat: "d/m/Y H:i",
    time_24hr: true,
    locale: "vn",
    minuteIncrement: 15,
    onChange: function (selectedDates, dateStr) {
        if (!selectedDates[0]) return;
        const d = selectedDates[0];
        const val = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}T${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
        document.getElementById('deadline-hidden').value = val;
    }
});
// Drag & drop
dropZone.addEventListener('dragover', e => {
    e.preventDefault();
    dropZone.classList.add('dragover');
});
dropZone.addEventListener('dragleave', () => dropZone.classList.remove('dragover'));
dropZone.addEventListener('drop', e => {
    e.preventDefault();
    dropZone.classList.remove('dragover');
    addFiles(e.dataTransfer.files);
});

fileInput.addEventListener('change', () => addFiles(fileInput.files));

function addFiles(files) {
    Array.from(files).forEach(f => {
        if (f.size > 5 * 1024 * 1024) {
            alert(`File "${f.name}" vượt quá 5MB`);
            return;
        }
        if (!selectedFiles.find(x => x.name === f.name && x.size === f.size)) {
            selectedFiles.push(f);
        }
    });
    renderFileList();
    syncFileInput();
}

function removeFile(idx) {
    selectedFiles.splice(idx, 1);
    renderFileList();
    syncFileInput();
}

function getExt(name) {
    return name.split('.').pop().toUpperCase();
}

function getIconClass(name) {
    const ext = name.split('.').pop().toLowerCase();
    if (['png', 'jpg', 'jpeg', 'gif', 'webp'].includes(ext)) return 'cf-file-icon--img';
    if (ext === 'pdf') return 'cf-file-icon--pdf';
    return '';
}

function formatSize(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
}

function renderFileList() {
    fileList.innerHTML = '';
    selectedFiles.forEach((f, i) => {
        const item = document.createElement('div');
        item.className = 'cf-file-item';
        item.innerHTML = `
    <div class="cf-file-icon ${getIconClass(f.name)}">${getExt(f.name)}</div>
    <div class="cf-file-info">
        <div class="cf-file-name">${f.name}</div>
        <div class="cf-file-size">${formatSize(f.size)}</div>
    </div>
    <button type="button" class="cf-file-remove" onclick="removeFile(${i})" title="Xóa">
        <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
            <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
        </svg>
    </button>`;
        fileList.appendChild(item);
    });
}

function syncFileInput() {
    const dt = new DataTransfer();
    selectedFiles.forEach(f => dt.items.add(f));
    fileInput.files = dt.files;
}
/* ===== USER SELECT ===== */
(function () {
    const wrapper = document.getElementById('usWrapper');
    if (!wrapper) return;

    const trigger = document.getElementById('usTrigger');
    const hiddenInput = document.getElementById('assignedToInput');
    const selectedEl = document.getElementById('usSelected');
    const searchInput = document.getElementById('usSearch');
    const list = document.getElementById('usList');
    const errEl = document.getElementById('assignedToErr');

    let focusIdx = -1;

    // Open / close
    function open() {
        wrapper.classList.add('is-open');
        searchInput.value = '';
        filter('');
        focusIdx = -1;
        setTimeout(() => searchInput.focus(), 40);
    }
    function close() {
        wrapper.classList.remove('is-open');
        focusIdx = -1;
    }

    trigger.addEventListener('click', e => {
        e.stopPropagation();
        wrapper.classList.contains('is-open') ? close() : open();
    });
    document.addEventListener('click', e => {
        if (!wrapper.contains(e.target)) close();
    });

    // Filter
    searchInput.addEventListener('input', () => {
        filter(searchInput.value.trim().toLowerCase());
        focusIdx = -1;
        clearFocus();
    });

    function filter(q) {
        let any = false;
        list.querySelectorAll('.us-item').forEach(item => {
            if (!q) { item.classList.remove('is-hidden'); any = true; return; }
            const match = (item.dataset.name || '').includes(q)
                || (item.dataset.email || '').includes(q);
            item.classList.toggle('is-hidden', !match);
            if (match) any = true;
        });

        let noRes = list.querySelector('.us-no-result');
        if (!any) {
            if (!noRes) {
                noRes = document.createElement('li');
                noRes.className = 'us-no-result';
                noRes.textContent = 'Không tìm thấy kết quả';
                list.appendChild(noRes);
            }
            noRes.style.display = 'block';
        } else if (noRes) {
            noRes.style.display = 'none';
        }
    }

    // Select
    list.addEventListener('click', e => {
        const item = e.target.closest('.us-item');
        if (!item || item.classList.contains('is-hidden')) return;
        pick(item);
        close();
    });

    function pick(item) {
        hiddenInput.value = item.dataset.value || '';
        if (errEl) errEl.textContent = '';

        if (!item.dataset.value) {
            selectedEl.innerHTML = '<span class="us-placeholder">— Chưa phân công —</span>';
        } else {
            const av = item.querySelector('.us-avatar').cloneNode(true);
            const info = item.querySelector('.us-info').cloneNode(true);
            selectedEl.innerHTML = '';
            selectedEl.appendChild(av);
            selectedEl.appendChild(info);
        }

        list.querySelectorAll('.us-item').forEach(o =>
            o.classList.toggle('is-selected', o === item)
        );
    }

    // Keyboard
    searchInput.addEventListener('keydown', e => {
        const visible = [...list.querySelectorAll('.us-item:not(.is-hidden)')];
        if (!visible.length) return;

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            focusIdx = Math.min(focusIdx + 1, visible.length - 1);
            setFocus(visible);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            focusIdx = Math.max(focusIdx - 1, 0);
            setFocus(visible);
        } else if (e.key === 'Enter') {
            e.preventDefault();
            if (focusIdx >= 0 && visible[focusIdx]) { pick(visible[focusIdx]); close(); }
        } else if (e.key === 'Escape') {
            close(); trigger.focus();
        }
    });

    function setFocus(visible) {
        clearFocus();
        const t = visible[focusIdx];
        if (t) { t.classList.add('is-focused'); t.scrollIntoView({ block: 'nearest' }); }
    }
    function clearFocus() {
        list.querySelectorAll('.us-item').forEach(o => o.classList.remove('is-focused'));
    }

    // Validate khi submit form
    const form = document.getElementById('cf-form');
    if (form) {
        form.addEventListener('submit', e => {
            if (!hiddenInput.value && errEl) {
                errEl.textContent = 'Vui lòng chọn người thực hiện';
                e.preventDefault();
            }
        });
    }
})();