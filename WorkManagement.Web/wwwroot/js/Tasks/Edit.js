
const ANTIFORGERY = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';

// ── Load files khi trang load ────────────────────────
window.addEventListener('DOMContentLoaded', () => renderFilesTable());
const startHidden = document.getElementById('startDate-hidden');
const deadlineHidden = document.getElementById('deadline-hidden');

if (START_DATE_VALUE && startHidden) startHidden.value = START_DATE_VALUE;
if (DEADLINE_VALUE && deadlineHidden) deadlineHidden.value = DEADLINE_VALUE;
const START_DATE_FLAT = START_DATE_VALUE ? START_DATE_VALUE.replace('T', ' ') : null;
const DEADLINE_FLAT = DEADLINE_VALUE ? DEADLINE_VALUE.replace('T', ' ') : null;
flatpickr("#startDate-picker", {
    enableTime: true,
    dateFormat: "d/m/Y H:i",
    time_24hr: true,
    locale: "vn",
    minuteIncrement: 15,
    onReady: function (selectedDates, dateStr, instance) {
        if (START_DATE_VALUE) {
            const [datePart, timePart] = START_DATE_VALUE.split('T');
            const [year, month, day] = datePart.split('-');
            const [hour, minute] = timePart.split(':');
            instance.setDate(new Date(year, month - 1, day, hour, minute), false);
        }
    },
    onChange: function (selectedDates) {
        if (!selectedDates[0]) return;
        const d = selectedDates[0];
        document.getElementById('startDate-hidden').value =
            `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}T${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
    }
});

flatpickr("#deadline-picker", {
    enableTime: true,
    dateFormat: "d/m/Y H:i",
    time_24hr: true,
    locale: "vn",
    minuteIncrement: 15,
    onReady: function (selectedDates, dateStr, instance) {
        if (DEADLINE_VALUE) {
            const [datePart, timePart] = DEADLINE_VALUE.split('T');
            const [year, month, day] = datePart.split('-');
            const [hour, minute] = timePart.split(':');
            instance.setDate(new Date(year, month - 1, day, hour, minute), false);
        }
    },
    onChange: function (selectedDates) {
        if (!selectedDates[0]) return;
        const d = selectedDates[0];
        document.getElementById('deadline-hidden').value =
            `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}T${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
    }
});

async function renderFilesTable() {
    const res = await fetch(`/Tasks/${TASK_ID}/files`);
    if (!res.ok) return;
    const files = await res.json();

    const tbody = document.getElementById('files-table-body');
    if (!tbody) return;

    if (files.length === 0) {
        tbody.innerHTML = `
                    <tr>
                        <td colspan="4" style="padding:20px;text-align:center;color:#9ca3af;font-size:13px;">
                            Chưa có file nào
                        </td>
                    </tr>`;
        return;
    }

    tbody.innerHTML = files.map(f => {
        const ext = f.fileName.split('.').pop().toUpperCase();
        const iconCls = getIconClass(f.fileName);
        const canDelete = CURRENT_USER_ROLE <= 2 || f.uploadedByUser === CURRENT_USER_ID;
        const roleTag = f.uploadedByRole <= 2
            ? `<span style="font-size:11px;padding:2px 7px;border-radius:20px;background:#ede9fe;color:#5b21b6;font-weight:600;">Manager</span>`
            : `<span style="font-size:11px;padding:2px 7px;border-radius:20px;background:#dbeafe;color:#1d4ed8;font-weight:600;">User</span>`;
        const date = new Date(f.uploadedAt).toLocaleDateString('vi-VN', {
            day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
        return `
                    <tr style="border-bottom:0.5px solid #f3f4f6;">
                        <td style="padding:10px 12px;">
                            <div style="display:flex;align-items:center;gap:8px;">
                                <div class="cf-file-icon ${iconCls}" style="width:28px;height:28px;font-size:9px;flex-shrink:0;">${ext}</div>
                                <div>
                                    <div style="font-size:13px;font-weight:600;color:#111827;">${f.fileName}</div>
                                    <div style="font-size:11px;color:#9ca3af;font-family:'DM Mono',monospace;">${formatSize(f.fileSize)}</div>
                                </div>
                            </div>
                        </td>
                        <td style="padding:10px 12px;">
                            <div style="font-size:13px;color:#374151;">${f.uploadedByName}</div>
                            <div style="margin-top:3px;">${roleTag}</div>
                        </td>
                        <td style="padding:10px 12px;font-size:12.5px;color:#6b7280;font-family:'DM Mono',monospace;">${date}</td>
                        <td style="padding:10px 12px;text-align:right;">
                            <a href="/Tasks/download/${f.id}" download
                               style="display:inline-flex;align-items:center;gap:4px;font-size:12.5px;font-weight:600;color:#4f46e5;text-decoration:none;padding:5px 10px;border-radius:7px;border:0.5px solid #e5e7eb;background:#fff;transition:background .15s;"
                               onmouseover="this.style.background='#f5f3ff'" onmouseout="this.style.background='#fff'">
                                <svg width="12" height="12" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                                    <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/>
                                    <polyline points="7 10 12 15 17 10"/>
                                    <line x1="12" y1="15" x2="12" y2="3"/>
                                </svg>
                                Tải về
                            </a>
                             ${canDelete ? `
            <button type="button" class="cf-file-remove" onclick="deleteFile(${f.id})"
                style="display:inline-flex;align-items:center;padding:5px 7px;border-radius:7px;border:0.5px solid #fee2e2;background:#fff;color:#991b1b;cursor:pointer;transition:background .15s;"
                onmouseover="this.style.background='#fee2e2'" onmouseout="this.style.background='#fff'">
                <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                    <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
            </button>` : ''}
                        </td>
                    </tr>`;
    }).join('');
}
function renderDlFile(f) {
    const ext = f.fileName.split('.').pop().toUpperCase();
    return `
                <a href="/Tasks/download/${f.id}" class="cf-file-dl" download>
                    <div class="cf-file-icon ${getIconClass(f.fileName)}">${ext}</div>
                    <span class="cf-file-dl-name">${f.fileName}</span>
                    <span class="cf-file-dl-size">${formatSize(f.fileSize)}</span>
                    <svg width="13" height="13" fill="none" stroke="#9ca3af" stroke-width="2" viewBox="0 0 24 24">
                        <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/>
                        <polyline points="7 10 12 15 17 10"/>
                        <line x1="12" y1="15" x2="12" y2="3"/>
                    </svg>
                </a>`;
}

// ── Drag & drop (dùng chung cho cả Manager lẫn User) ─
let selectedFiles = [];
document.getElementById('submitModal').addEventListener('click', e => {
    if (e.target === e.currentTarget) closeSubmitModal();
});
const dropZoneEl = document.getElementById('dropZone');
const fileInputEl = document.getElementById('fileInput');

if (dropZoneEl && fileInputEl) {
    dropZoneEl.addEventListener('dragover', e => { e.preventDefault(); dropZoneEl.classList.add('dragover'); });
    dropZoneEl.addEventListener('dragleave', () => dropZoneEl.classList.remove('dragover'));
    dropZoneEl.addEventListener('drop', e => {
        e.preventDefault(); dropZoneEl.classList.remove('dragover');
        addFiles(e.dataTransfer.files);
    });
    fileInputEl.addEventListener('change', () => addFiles(fileInputEl.files));
}

function addFiles(files) {
    Array.from(files).forEach(f => {
        if (f.size > 5 * 1024 * 1024) { showAlert('warning', `File "${f.name}" vượt quá 5MB`); return; }
        if (!selectedFiles.find(x => x.name === f.name && x.size === f.size)) selectedFiles.push(f);
    });
    renderFileList();
    if (CAN_EDIT) syncFileInput();
}

function removeFile(i) {
    selectedFiles.splice(i, 1);
    renderFileList();
    if (CAN_EDIT) syncFileInput();
}

function renderFileList() {
    const list = document.getElementById('fileList');
    if (!list) return;
    list.innerHTML = selectedFiles.map((f, i) => `
                <div class="cf-file-item">
                    <div class="cf-file-icon ${getIconClass(f.name)}">${f.name.split('.').pop().toUpperCase()}</div>
                    <div class="cf-file-info">
                        <div class="cf-file-name">${f.name}</div>
                        <div class="cf-file-size">${formatSize(f.size)}</div>
                    </div>
                    <button type="button" class="cf-file-remove" onclick="removeFile(${i})">
                        <svg width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                        </svg>
                    </button>
                </div>`).join('');
}

function syncFileInput() {
    const dt = new DataTransfer();
    selectedFiles.forEach(f => dt.items.add(f));
    if (fileInputEl) fileInputEl.files = dt.files;
}

async function saveTask() {
    if (selectedFiles.length > 0) {
        const fd = new FormData();
        fd.append('__RequestVerificationToken', ANTIFORGERY());
        selectedFiles.forEach(f => fd.append('files', f));
        const res = await fetch(`/Tasks/${TASK_ID}/upload`, { method: 'POST', body: fd });
        if (!res.ok) {
            showAlert('error', await res.text() || 'Lỗi khi lưu file.');
            return;
        }
    }

    if (CAN_EDIT) {
        document.getElementById('cf-form').submit();
        return;
    }

    const statusSelect = document.getElementById('status-select');
    if (statusSelect) {
        document.getElementById('user-status-input').value = statusSelect.value;
    }
    document.getElementById('user-status-form').submit();
}
async function doSubmitTask() {
    closeSubmitModal();
    if (selectedFiles.length > 0) {
        const fd = new FormData();
        fd.append('__RequestVerificationToken', ANTIFORGERY());
        selectedFiles.forEach(f => fd.append('files', f));
        const upRes = await fetch(`/Tasks/${TASK_ID}/upload`, { method: 'POST', body: fd });
        if (!upRes.ok) {
            showAlert('error', 'Lỗi khi upload file.');
            return;
        }
    }
    const fd2 = new FormData();
    fd2.append('__RequestVerificationToken', ANTIFORGERY());
    const res = await fetch(`/Tasks/${TASK_ID}/submit`, { method: 'POST', body: fd2 });
    if (res.ok) {
        showAlert('success', 'Đã gửi đánh giá!');
        setTimeout(() => location.href = '/Tasks/Index', 800);
    } else {
        showAlert('error', await res.text() || 'Có lỗi xảy ra.');
    }
}
let isApprove = true;

function selectReview(value) {
    isApprove = value;

    const approve = document.getElementById("rv-approve");
    const reject = document.getElementById("rv-reject");
    const reasonWrap = document.getElementById("rv-reason-wrap");

    // reset style
    approve.style.borderColor = "#e5e7eb";
    reject.style.borderColor = "#e5e7eb";

    if (value) {
        approve.style.borderColor = "#16a34a"; // xanh
        reasonWrap.style.display = "none";
    } else {
        reject.style.borderColor = "#ef4444"; // đỏ
        reasonWrap.style.display = "block";
    }
}
async function doReview() {
    const taskId = document.getElementById("rv-task-id").value;
    const reason = document.getElementById("rv-reason").value.trim();
    const err = document.getElementById("rv-reason-err");
    const btn = document.getElementById("rv-submit-btn");
    err.innerText = "";

    if (!isApprove && !reason) {
        err.innerText = "Vui lòng nhập lý do từ chối";
        return;
    }

    btn.disabled = true;
    btn.innerText = "Đang xử lý...";

    try {
        const fd = new FormData();
        fd.append('__RequestVerificationToken', ANTIFORGERY());
        fd.append('Approved', isApprove);
        fd.append('RejectedReason', reason || "");

        const res = await fetch(`/tasks/${taskId}/review`, { method: 'POST', body: fd });
        if (!res.ok) {
            const text = await res.text();
            throw new Error(text || "Lỗi server");
        }

        closeReviewModal();
        showAlert('success', isApprove ? "Đã duyệt công việc." : "Đã từ chối công việc.");
        setTimeout(() => location.href = "/Tasks/Index", 800);
    } catch (e) {
        err.innerText = "Không thể gửi đánh giá";
        showAlert('error', e.message || 'Có lỗi xảy ra.');
    } finally {
        btn.disabled = false;
        btn.innerText = "Xác nhận đánh giá";
    }
}
async function deleteFile(fileId) {
    showAlert('warning', 'Đang xóa file...');

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    const res = await fetch(`/Tasks/files/${fileId}`, {
        method: 'DELETE',
        headers: {
            'RequestVerificationToken': token
        }
    });

    if (res.ok) {
        showAlert('success', 'Đã xóa file thành công.');
        await renderFilesTable(); // reload lại bảng
    } else {
        const msg = await res.text();
        showAlert('error', msg || 'Xóa file thất bại.');
    }
}
// Open Modal
function submitTask() {
    document.getElementById('submitModal').classList.add('open');
    document.body.style.overflow = 'hidden';
}

function closeSubmitModal() {
    document.getElementById('submitModal').classList.remove('open');
    document.body.style.overflow = '';
}
function openReviewModal() {
    document.getElementById("reviewModal").style.display = "flex";
}

function closeReviewModal() {
    document.getElementById("reviewModal").style.display = "none";
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