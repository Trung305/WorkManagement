
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
