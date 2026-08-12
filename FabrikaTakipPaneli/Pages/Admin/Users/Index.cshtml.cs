using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FabrikaTakipPaneli.Authorization;
using FabrikaTakipPaneli.Data;

namespace FabrikaTakipPaneli.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public IList<UserListItem> Users { get; set; } = new List<UserListItem>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        var currentUserId = _userManager.GetUserId(User);

        var allUsers = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var items = new List<UserListItem>();
        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserListItem
            {
                Id = user.Id,
                Email = user.Email,
                CurrentRole = roles.FirstOrDefault(),
                IsCurrentUser = user.Id == currentUserId
            });
        }

        Users = items;
    }

    public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string role)
    {
        if (!AppRoles.All.Contains(role))
        {
            StatusMessage = "Geçersiz rol.";
            return RedirectToPage();
        }

        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId)
        {
            StatusMessage = "Kendi rolünüzü değiştiremezsiniz.";
            return RedirectToPage();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            StatusMessage = "Kullanıcı bulunamadı.";
            return RedirectToPage();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        StatusMessage = $"{user.Email} kullanıcısının rolü '{role}' olarak güncellendi.";
        return RedirectToPage();
    }

    public class UserListItem
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? CurrentRole { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
