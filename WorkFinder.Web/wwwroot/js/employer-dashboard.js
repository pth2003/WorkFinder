/**
 * Employer Dashboard JavaScript
 * Xử lý các tương tác AJAX cho phần Employer Dashboard
 */

// Hiển thị loading overlay cho toàn trang
function showPageLoading() {
    const loadingOverlay = document.getElementById('page-loading-overlay');
    console.log('Showing loading overlay, element:', loadingOverlay);
    
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
    console.log('Hiding loading overlay, element:', loadingOverlay);
    
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
            console.warn('Loading protection triggered - loading shown for too long');
            hidePageLoading();
        }
    }, 2000);
}

// Xử lý tác vụ không cần refresh trang
async function handleAjaxAction(url, method = 'GET', data = null) {
    return window.loadingHandler.withLoading(async () => {
        const options = {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        };
        
        if (data && (method === 'POST' || method === 'PUT')) {
            options.body = JSON.stringify(data);
        }
        
        const response = await fetch(url, options);
        
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        
        let jsonResult;
        try {
            jsonResult = await response.json();
        } catch (jsonError) {
            console.error('JSON parse error:', jsonError);
            throw new Error('Invalid response format');
        }
        
        return jsonResult;
    });
}

// Hiển thị thông báo toast
function showToast(title, message, type = 'success') {
    // Implementation depends on your toast library
    // Example with Bootstrap 5 toast
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) return;
    
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : 'success'} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <strong>${title}</strong>: ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    
    // Kiểm tra Bootstrap có sẵn không
    if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
        
        // Auto remove after hidden
        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    } else {
        // Fallback nếu không có Bootstrap
        toast.style.display = 'block';
        setTimeout(() => {
            toast.remove();
        }, 5000);
    }
}

// Load dữ liệu Dashboard Stats và Recent Jobs cùng lúc
async function loadDashboardData() {

    try {
        // Tạo promises cho các request cần thực hiện
        const promises = [
            fetch('/Employer/Home/GetDashboardStats', {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            }),
            fetch('/Employer/Home/GetRecentJobs', {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
        ]
        // Thực hiện tất cả các request cùng lúc
        const responses = await Promise.all(promises);
        // Xử lý kết quả cho dashboard stats
        if (responses[0].ok) {
            try {
                const statsResult = await responses[0].json();
                updateDashboardStats(statsResult);
                window.loadingHandler.hidePageLoading();
            } catch (error) {
                console.error('Failed to parse dashboard stats JSON:', error);
            }
        } else {
            console.error('Failed to load dashboard stats, status:', responses[0].status);
        }
        
        // Xử lý kết quả cho recent jobs
        if (responses[1].ok) {
            try {
                const jobsResult = await responses[1].json();
                window.loadingHandler.hidePageLoading();
                updateRecentJobs(jobsResult);
            } catch (error) {
                console.error('Failed to parse recent jobs JSON:', error);
            }
        } else {
            console.error('Failed to load recent jobs, status:', responses[1].status);
        }
    } catch (error) {
        console.error('Failed to load dashboard data:', error);
        showToast('Error', 'Failed to load dashboard data. Please try again.', 'error');
    }
}





// Cập nhật phần Dashboard Stats từ dữ liệu trả về
function updateDashboardStats(result) {
    if (!result || !result.success) {
        console.error('Error loading stats:', result ? result.message : 'Invalid response');
        return;
    }
    
    // Đảm bảo dữ liệu tồn tại trước khi truy cập
    if (!result.data) {
        console.error('Missing data in stats response');
        return;
    }
    
    // Cập nhật phần chào mừng
    const greetingEl = document.getElementById('dashboard-greeting');
    if (greetingEl) {
        const companyName = result.data.companyName || 'User';
        greetingEl.innerHTML = `
            <h3>Hello, ${companyName}</h3>
            <p class="text-muted">Here is your daily activities and applications</p>
        `;
    }
    
    // Cập nhật phần thống kê
    const statsEl = document.getElementById('stats-section');
    if (statsEl) {
        const activeJobs = result.data.activeJobs || 0;
        const totalApplications = result.data.totalApplications || 0;
        
        statsEl.innerHTML = `
        <div class="col-md-6">
            <div class="stats-card">
                <div class="stats-content">
                        <h2>${activeJobs}</h2>
                    <p>Open Jobs</p>
                </div>
                <div class="stats-icon">
                    <i class="fas fa-briefcase"></i>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="stats-card">
                <div class="stats-content">
                        <h2>${totalApplications}</h2>
                        <p>Saved Candidates</p>
                </div>
                <div class="stats-icon">
                    <i class="fas fa-user"></i>
                </div>
            </div>
        </div>
    `;
    }
}

// Cập nhật phần Recent Jobs từ dữ liệu trả về
function updateRecentJobs(result) {
    if (!result || !result.success) {
        console.error('Error loading jobs:', result ? result.message : 'Invalid response');
        return;
    }
    
    // Cập nhật bảng công việc
    const jobsEl = document.getElementById('recent-jobs-section');
    if (!jobsEl) return;
    
    if (result.data && result.data.length > 0) {
        let tableHtml = `
        <table class="table jobs-table">
            <thead>
                <tr>
                        <th>JOBS</th>
                        <th>STATUS</th>
                        <th>APPLICATIONS</th>
                        <th>ACTIONS</th>
                </tr>
            </thead>
            <tbody>
    `;
    
        result.data.forEach(job => {
            const title = job.title || 'Untitled';
            const jobType = job.jobType || 'Unknown';
            const daysRemaining = job.daysRemaining || 0;
            const isActive = !!job.isActive;
            const applicationCount = job.applicationCount || 0;
            const id = job.id || 0;
            
            tableHtml += `
                <tr>
                    <td class="job-info">
                        <h6>${title}</h6>
                        <div class="job-meta">
                            <span class="job-type">${jobType}</span>
                            <span class="separator">•</span>
                            <span class="time-remaining">${daysRemaining} days remaining</span>
                        </div>
                    </td>
                    <td>
                        <span class="badge bg-${isActive ? "success" : "secondary"}">
                            ${isActive ? "Active" : "Expired"}
                        </span>
                    </td>
                    <td>
                        <div class="applications-count">
                            <i class="fas fa-users me-2"></i>
                            <span>${applicationCount} Applications</span>
                    </div>
                </td>
                <td>
                        <div class="dropdown">
                            <button class="btn btn-view-applications" id="dropdownMenuButton${id}"
                                data-bs-toggle="dropdown" aria-expanded="false">
                        View Applications
                            </button>
                            <div class="dropdown-menu dropdown-menu-end"
                                aria-labelledby="dropdownMenuButton${id}">
                                <a class="dropdown-item" href="#">
                                    <i class="fas fa-bullhorn me-2"></i> Promote Job
                                </a>
                                <a class="dropdown-item" href="#">
                                    <i class="fas fa-eye me-2"></i> View Detail
                                </a>
                                <a class="dropdown-item" href="#">
                                    <i class="fas fa-times-circle me-2"></i> Mark as expired
                                </a>
                            </div>
                        </div>
                </td>
            </tr>
        `;
    });
    
        tableHtml += `
            </tbody>
        </table>
    `;
    
        jobsEl.innerHTML = tableHtml;
    } else {
        jobsEl.innerHTML = `
            <div class="p-4 text-center">
                <p>You haven't posted any jobs yet.</p>
                <a href="/Employer/Job/Create" class="btn btn-primary mt-2">
                    <i class="fas fa-plus-circle me-2"></i> Post a Job
                </a>
            </div>
        `;
    }
}

// Document ready
document.addEventListener('DOMContentLoaded', function() {
  
    // Thiết lập bảo vệ chống treo loading
    window.loadingHandler.setupLoadingProtection();
    
    // Tạo toast container nếu chưa có
    if (!document.getElementById('toast-container')) {
        
        const toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }
    
    // Kiểm tra nếu đang ở trang dashboard
    if (document.querySelector('.employers-dashboard')) {
        
        try {
            // Tải dữ liệu dashboard lần đầu
            loadDashboardData();
            setInterval(loadDashboardData, 300000); // 5 phút
        } catch (error) {
            console.error('Dashboard initialization error:', error);
            hidePageLoading(); // Đảm bảo loading được ẩn nếu có lỗi
        }
    } else {
        console.log("Not on employer dashboard page");
    }
    
    // Xử lý form job submission bằng AJAX
    const jobForm = document.getElementById('job-form');
    if (jobForm) {
        jobForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            // Validate form
            if (!jobForm.checkValidity()) {
                jobForm.classList.add('was-validated');
                return;
            }
            
            // Create FormData
            const formData = new FormData(jobForm);
            
            // Submit form via AJAX
            showPageLoading();
            
            try {
                const response = await fetch(jobForm.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });
                
                let result;
                try {
                    result = await response.json();
                } catch (jsonError) {
                    console.error('JSON parse error:', jsonError);
                    throw new Error('Invalid response format');
                }
                
                if (!response.ok) {
                    throw new Error(result && result.message ? result.message : 'Form submission failed');
                }
                
                if (result.success) {
                    showToast('Success', result.message || 'Job posted successfully!');
                    
                    // Redirect if needed
                    if (result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    }
                } else {
                    showToast('Error', result.message || 'Failed to post job.', 'error');
                }
            } catch (error) {
                console.error('Submit error:', error);
                showToast('Error', error.message || 'An unexpected error occurred.', 'error');
            } finally {
                hidePageLoading();
            }
        });
    }
}); 