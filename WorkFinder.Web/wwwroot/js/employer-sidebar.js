/**
 * Employer Sidebar Mobile Toggle with Bootstrap 5
 * Handles the mobile sidebar functionality for the employer area
 */

document.addEventListener('DOMContentLoaded', function() {
    // Elements
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');
    const employersDashboard = document.querySelector('.employers-dashboard');
    
    // Only proceed if we're in the employer dashboard
    if (!sidebar || !mainContent || !employersDashboard) return;
    
    // Use existing toggle button in header instead of creating a new one
    const toggleButton = document.querySelector('.navbar-toggler');
    
    // If no toggle button exists in header, exit early
    if (!toggleButton) return;
    
    // Create overlay for sidebar
    const overlay = document.createElement('div');
    overlay.className = 'sidebar-overlay';
    document.body.appendChild(overlay);
    
    // Add mobile classes
    sidebar.classList.add('mobile-hidden');
    mainContent.classList.add('full-width');
    
    // Toggle sidebar function
    toggleButton.addEventListener('click', function() {
        sidebar.classList.toggle('mobile-visible');
        overlay.classList.toggle('active');
        toggleButton.innerHTML = sidebar.classList.contains('mobile-visible') 
            ? '<i class="fas fa-times"></i>' 
            : '<i class="fas fa-bars"></i>';
    });
    
    // Close sidebar when clicking on overlay
    overlay.addEventListener('click', function() {
        sidebar.classList.remove('mobile-visible');
        overlay.classList.remove('active');
        toggleButton.innerHTML = '<i class="fas fa-bars"></i>';
    });
    
    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', function(event) {
        const isMobile = window.innerWidth < 768;
        const isClickInsideSidebar = sidebar.contains(event.target);
        const isClickOnToggleButton = toggleButton.contains(event.target);
        
        if (isMobile && !isClickInsideSidebar && !isClickOnToggleButton && sidebar.classList.contains('mobile-visible')) {
            sidebar.classList.remove('mobile-visible');
            overlay.classList.remove('active');
            toggleButton.innerHTML = '<i class="fas fa-bars"></i>';
        }
    });
    
    // Handle window resize
    window.addEventListener('resize', function() {
        if (window.innerWidth >= 768) {
            sidebar.classList.remove('mobile-hidden', 'mobile-visible');
            mainContent.classList.remove('full-width');
        } else {
            sidebar.classList.add('mobile-hidden');
            mainContent.classList.add('full-width');
            if (sidebar.classList.contains('mobile-visible')) {
                sidebar.classList.remove('mobile-visible');
                overlay.classList.remove('active');
                toggleButton.innerHTML = '<i class="fas fa-bars"></i>';
            }
        }
    });
})
;