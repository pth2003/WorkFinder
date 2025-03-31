/**
 * Loading Handler JavaScript
 * Xử lý hiển thị/ẩn loading overlay và bảo vệ chống treo loading
 */

// Hiển thị loading overlay cho toàn trang
function showPageLoading() {
    const loadingOverlay = document.getElementById('page-loading-overlay');  
    if (loadingOverlay) {
        // Đảm bảo hiển thị trên mọi browser
        loadingOverlay.style.display = 'flex';
        loadingOverlay.style.opacity = '1';
        loadingOverlay.style.visibility = 'visible';
        // Ghi nhớ thời điểm hiển thị để xử lý trường hợp bị kẹt
        window.lastLoadingShown = new Date().getTime();
    } else {
        console.error('Loading overlay element not found');
    }
}

// Ẩn loading overlay
function hidePageLoading() {
    const loadingOverlay = document.getElementById('page-loading-overlay');

    if (loadingOverlay) {
        // Đảm bảo ẩn trên mọi browser
        loadingOverlay.style.display = 'none';
        loadingOverlay.style.opacity = '0';
        loadingOverlay.style.visibility = 'hidden';
        // Xóa thời điểm hiển thị
        delete window.lastLoadingShown;
    } else {
        console.error('Loading overlay element not found');
    }
}

// Thêm timeout bảo vệ cho loading
function setupLoadingProtection() {
    // Tự động ẩn loading nếu hiển thị quá lâu (>10 giây)
    setInterval(() => {
        const lastShown = window.lastLoadingShown;
        if (lastShown && (new Date().getTime() - lastShown > 10000)) {
            console.warn('Loading protection triggered - hiển thị loading quá lâu');
            hidePageLoading();
        }
    }, 2000);
}

// Hàm wrapper để xử lý AJAX với loading
async function withLoading(ajaxFunction) {
    showPageLoading();
    try {
        const result = await ajaxFunction();
        return result;
    } finally {
        hidePageLoading();
    }
}

// Thiết lập interceptor cho các link click để hiển thị loading
function setupLinkInterceptor() {
    document.addEventListener('click', function(e) {
        // Kiểm tra xem click có phải vào thẻ a không
        const link = e.target.closest('a');
        if (!link) return;
        
        // Bỏ qua các link đặc biệt
        if (link.getAttribute('data-no-loading') === 'true' || 
            link.getAttribute('target') === '_blank' ||
            link.href.startsWith('javascript:') ||
            link.href.startsWith('#') ||
            link.href.includes('mailto:') ||
            link.href.includes('tel:')) {
            return;
        }
        
        // Kiểm tra xem link có dẫn đến trang khác không (trong cùng domain)
        const isSameDomain = link.hostname === window.location.hostname;
        
        if (isSameDomain) {
            // Hiển thị loading khi người dùng click vào link
            showPageLoading();
        }
    });
}

// Xử lý cho trạng thái 404 khi nạp trang
function handlePageLoad() {
    // Khi trang đã tải xong, ẩn loading overlay
    window.addEventListener('load', function() {
        hidePageLoading();
    });
    
    // Khi trang bị lỗi (không tìm thấy tài nguyên), ẩn loading
    window.addEventListener('error', function(e) {
        if (e.target.tagName === 'IMG' || e.target.tagName === 'SCRIPT' || e.target.tagName === 'LINK') {
            console.warn('Resource not found:', e.target.src || e.target.href);
        }
    }, true);
}

// Hàm khởi tạo tất cả các chức năng
function initialize() {
    setupLoadingProtection();
    setupLinkInterceptor();
    handlePageLoad();
}

// Khởi tạo khi DOM đã sẵn sàng
document.addEventListener('DOMContentLoaded', initialize);

// Export các hàm để sử dụng ở các file khác
window.loadingHandler = {
    showPageLoading,
    hidePageLoading,
    setupLoadingProtection,
    withLoading
}; 