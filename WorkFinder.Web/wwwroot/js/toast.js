function showToast(title, message, type = 'success') {
    // Implementation depends on your toast library
    // Example with Bootstrap 5 toast
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) return;
    
    const uniqueId = 'toast-' + new Date().getTime();
    const toast = document.createElement('div');
    toast.id = uniqueId;
    toast.className = `toast align-items-center border-0 mb-3`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    // Color based on type
    let backgroundColor, iconClass;
    switch(type) {
        case 'error':
            backgroundColor = '#dc3545';
            iconClass = 'fas fa-exclamation-triangle';
            break;
        case 'warning':
            backgroundColor = '#ffc107';
            iconClass = 'fas fa-exclamation-circle';
            break;
        case 'info':
            backgroundColor = '#0dcaf0';
            iconClass = 'fas fa-info-circle';
            break;
        case 'success':
        default:
            backgroundColor = '#198754';
            iconClass = 'fas fa-check-circle';
            break;
    }
    
    // Apply custom styling
    toast.style.backgroundColor = backgroundColor;
    toast.style.color = '#fff';
    toast.style.minWidth = '300px';
    toast.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.15)';
    toast.style.fontSize = '1.1rem';
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body d-flex align-items-center">
                <i class="${iconClass} me-2" style="font-size: 1.25rem;"></i>
                <div>
                    <strong style="font-size: 1.1rem;">${title}</strong>
                    <div>${message}</div>
                </div>
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" 
                    data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    
    // Kiểm tra Bootstrap có sẵn không
    if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
        const bsToast = new bootstrap.Toast(toast, {
            autohide: true,
            delay: 5000
        });
        bsToast.show();
        
        // Auto remove after hidden
        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    } else {
        // Fallback nếu không có Bootstrap
        toast.style.display = 'block';
        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transition = 'opacity 0.5s ease-out';
            setTimeout(() => {
                toast.remove();
            }, 500);
        }, 5000);
    }
}

window.toast = {
    showToast
}

