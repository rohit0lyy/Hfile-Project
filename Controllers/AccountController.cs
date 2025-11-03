using HFile.Data;
using HFile.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HFile.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Sign Up (GET)
        public IActionResult Signup()
        {
            return View();
        }

        // ✅ Sign Up (POST)
        [HttpPost]
        public IActionResult Signup(UserModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Users.Add(user);
                    _context.SaveChanges();
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    // Ye line error ka actual reason dikhayegi
                    Console.WriteLine(ex.InnerException?.Message);
                    ModelState.AddModelError("", "Error: " + ex.InnerException?.Message);
                }
            }
            return View(user);
        }
        // ✅ Login (GET)Microsoft.EntityFrameworkCore.DbUpdateException: 'An error occurred while saving the entity changes. See 
        public IActionResult Login()
        {
            return View();
        }

        // ✅ Login (POST)
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                if (user != null)
                {
                    ViewBag.UserName = user.FullName;
                    ViewBag.ProfileImage = user.Imageprofile ?? "";
                }
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetInt32("UId", user.Id);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid Email or Password!";
            return View();
        }

        // ✅ Dashboard
        public IActionResult Dashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserName")))
                return RedirectToAction("Login");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile FileUpload, string FileName, int Uploaded_by)
        {
            if (FileUpload != null && FileUpload.Length > 0)
            {
                // Save path
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                int uploadedBy = HttpContext.Session.GetInt32("UId") ?? 0;
                //// Unique file name
                //var uniqueFileName = Path.GetFileNameWithoutExtension(FileUpload.FileName)
                //                     + "_" + Guid.NewGuid().ToString().Substring(0, 8)
                //                     + Path.GetExtension(FileUpload.FileName);

                var filePath = Path.Combine(uploadsFolder, FileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await FileUpload.CopyToAsync(fileStream);
                }

                // Save file info to database using EF
                var uploadFileRecord = new UploadFileModels
                {
                    FileName = FileName,
                    File_extension = Path.GetExtension(FileUpload.FileName),
                    Uploaded_by = uploadedBy, // pass user id
                    Uploaded_Date = DateTime.Now,
                    IsDeleted = false
                };

                _context.DocumentDetails.Add(uploadFileRecord);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "File uploaded and saved to database successfully!";
                return RedirectToAction("Dashboard", "Account");
            }

            TempData["ErrorMessage"] = "Please select a valid file!";
            return RedirectToAction("Dashboard", "Account");
        }

        // Convenience wrapper if you want an AddFile endpoint that uses session UId and reuses UploadFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFile(IFormFile FileUpload, string FileName)
        {
            int uploadedBy = HttpContext.Session.GetInt32("UId") ?? 0;
            return await UploadFile(FileUpload, FileName, uploadedBy);
        }

        // ✅ Partial View to display uploaded files
        public async Task<IActionResult> _FileListPartial()
        {
            var files = await _context.DocumentDetails
                .Where(f => !f.IsDeleted)
                .OrderByDescending(f => f.Uploaded_Date)
                .ToListAsync();

            return PartialView("_FileListPartial", files);
        }

        // GET: Edit file metadata / show edit form
        [HttpGet]
        public async Task<IActionResult> EditFile(int id)
        {
            var file = await _context.DocumentDetails.FindAsync(id);
            if (file == null || file.IsDeleted)
                return NotFound();

            // You can return a dedicated view "EditFile" with the file model.
            return View("EditFile", file);
        }

        // POST: Edit file metadata and optionally replace uploaded file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFile(int id, string FileName, IFormFile? NewFileUpload)
        {
            var fileRecord = await _context.DocumentDetails.FindAsync(id);
            if (fileRecord == null || fileRecord.IsDeleted)
                return NotFound();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Replace physical file if a new file was uploaded
            if (NewFileUpload != null && NewFileUpload.Length > 0)
            {
                // Try to delete previous file (both conventions: with and without extension)
                try
                {
                    var oldPath1 = Path.Combine(uploadsFolder, fileRecord.FileName ?? "");
                    var oldPath2 = Path.Combine(uploadsFolder, (fileRecord.FileName ?? "") + (fileRecord.File_extension ?? ""));
                    if (System.IO.File.Exists(oldPath1))
                        System.IO.File.Delete(oldPath1);
                    if (System.IO.File.Exists(oldPath2))
                        System.IO.File.Delete(oldPath2);
                }
                catch
                {
                    // swallow file-delete exceptions, but you may want to log
                }

                // Save new file using same convention as original UploadFile (FileName used as filename)
                var newFilePath = Path.Combine(uploadsFolder, FileName);
                using (var fs = new FileStream(newFilePath, FileMode.Create))
                {
                    await NewFileUpload.CopyToAsync(fs);
                }

                fileRecord.File_extension = Path.GetExtension(NewFileUpload.FileName);
            }

            // Update metadata
            fileRecord.FileName = FileName;
            _context.DocumentDetails.Update(fileRecord);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "File updated successfully.";
            return RedirectToAction("Dashboard");
        }

        // POST: Soft-delete file (mark IsDeleted = true) and attempt to remove physical file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _context.DocumentDetails.FindAsync(id);
            if (file == null)
                return NotFound();

            file.IsDeleted = true;
            _context.DocumentDetails.Update(file);
            await _context.SaveChangesAsync();

            // Try to delete physical file if exists (both filename conventions)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            try
            {
                var path1 = Path.Combine(uploadsFolder, file.FileName ?? "");
                var path2 = Path.Combine(uploadsFolder, (file.FileName ?? "") + (file.File_extension ?? ""));
                if (System.IO.File.Exists(path1))
                    System.IO.File.Delete(path1);
                else if (System.IO.File.Exists(path2))
                    System.IO.File.Delete(path2);
            }
            catch
            {
                // ignore file system errors; consider logging if needed
            }

            TempData["SuccessMessage"] = "File deleted.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult UpdateProfile()
        {
            // Model/ViewBag setup
            int? userId = HttpContext.Session.GetInt32("UId");

            if (userId != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    ViewBag.UserName = user.FullName ?? "User";
                    ViewBag.Email = user.Email ?? "";
                    ViewBag.Gender = user.Gender ?? "";
                    ViewBag.Phone = user.Phone ?? "";
                    ViewBag.ProfileImage = user.Imageprofile ?? null;
                }
                else
                {
                    ViewBag.UserName = "Unknown";
                }
            }
            else
            {
                ViewBag.UserName = "Guest";
            }
            return View();
        }
        [HttpPost]
        public IActionResult UpdateProfile(string Email, string Gender, string Phone)
        {
            // Yahan database update logic likhna hai
            ViewBag.Message = "Profile Updated Successfully!";
            return View("Dashboard");
        }

        // ✅ Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }



        [HttpPost]
        public JsonResult UploadProfilePhoto(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    string folderPath = Path.Combine("D:\\Mangal\\WebAppTest\\HFile\\wwwroot\\img", "UserImages");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    //string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    // Optional: Save file name in DB
                    int? userId = HttpContext.Session.GetInt32("UId");
                    var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                    if (user != null)
                    {
                        user.Imageprofile = fileName;
                        _context.SaveChanges();
                    }

                    return Json(new { success = true, fileName });
                }

                return Json(new { success = false, message = "No file selected." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}