using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationRepository notificationRepository,
            IAuthService authService,
            ILogger<NotificationsController> logger)
        {
            _notificationRepository = notificationRepository;
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // This route will be handled by the Dashboard controller's Notifications action
            return RedirectToAction("Notifications", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized();
                }

                var notification = await _notificationRepository.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound();
                }

                // Convert userId to int to match notification's UserId
                if (notification.UserId != Convert.ToInt32(user.Id))
                {
                    return Forbid();
                }

                // Mark as read
                var result = await _notificationRepository.MarkAsReadAsync(id);
                if (result)
                {
                    // If the notification has a link, redirect to it
                    if (!string.IsNullOrEmpty(notification.Link))
                    {
                        return Redirect(notification.Link);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Could not mark notification as read.";
                }

                return RedirectToAction("Notifications", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                TempData["ErrorMessage"] = "An error occurred while marking the notification as read.";
                return RedirectToAction("Notifications", "Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized();
                }

                var result = await _notificationRepository.MarkAllAsReadForUserAsync(user.Id.ToString());
                if (result)
                {
                    TempData["SuccessMessage"] = "All notifications marked as read.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Could not mark all notifications as read.";
                }

                return RedirectToAction("Notifications", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                TempData["ErrorMessage"] = "An error occurred while marking all notifications as read.";
                return RedirectToAction("Notifications", "Dashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized();
                }

                var notification = await _notificationRepository.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return NotFound();
                }

                // Convert userId to int to match notification's UserId
                if (notification.UserId != Convert.ToInt32(user.Id))
                {
                    return Forbid();
                }

                var result = await _notificationRepository.DeleteNotificationAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Notification deleted.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Could not delete notification.";
                }

                return RedirectToAction("Notifications", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                TempData["ErrorMessage"] = "An error occurred while deleting the notification.";
                return RedirectToAction("Notifications", "Dashboard");
            }
        }

        [HttpPost]
        [Route("Notifications/MarkAsReadAjax/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReadAjax(int id)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var notification = await _notificationRepository.GetNotificationByIdAsync(id);
                if (notification == null)
                {
                    return Json(new { success = false, message = "Notification not found" });
                }

                // Convert userId to int to match notification's UserId
                if (notification.UserId != Convert.ToInt32(user.Id))
                {
                    return Json(new { success = false, message = "Unauthorized access" });
                }

                // Mark as read
                var result = await _notificationRepository.MarkAsReadAsync(id);
                if (result)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Could not mark notification as read" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return Json(new { success = false, message = "An error occurred" });
            }
        }
    }
}