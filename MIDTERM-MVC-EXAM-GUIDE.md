# MVC Midterm “Build-It-From-Scratch” Guide (2 related models + 1 controller + 2 functions + 2 views)

This document is designed so you can walk into the exam, start from a blank MVC project, and finish what your professor described **without needing anything beyond this doc + your class project files**.

It follows the professor’s constraints from `transc.txt`:
- New **ASP.NET Core MVC** project
- **No Identity / no user accounts**
- **Two related models** (ex: Course ↔ Instructor)
- **One controller**
- **Only two functionalities** (ex: Index + Create) and therefore **two views**
- Use **field-level validation only** (tag helper `asp-validation-for`, optional `_ValidationScriptsPartial`)
- You may need **a dropdown** to select the related record (use ViewModel OR ViewBag OR ViewData — pick one)
- Be able to do **migrations** and **update database**
- Possibly add **seed data** (a couple rows per model)

---

## 0) Pick your exam plan (do this first)
Your professor will choose which “two functionalities” you must implement. The most common patterns:

### Option A (most common): **Index + Create**
- `Index`: show list of records
- `Create`: create a record (and if needed, choose related record via dropdown)

### Option B: **Index + Edit**
- `Index`: list
- `Edit`: update one record

### Option C: **Index + Delete**
- `Index`: list
- `Delete`: confirm + delete

If you can do **Index + Create** well, you can usually adapt fast to Edit/Delete.

---

## 1) Create the project (EXAM SAFE SETTINGS)
1. Visual Studio → **Create a new project**
2. Template: **ASP.NET Core Web App (Model-View-Controller)**
3. Name it anything (use something recognizable)
4. **Authentication: None** (important)
5. Target framework: whatever he specifies (he mentioned “.NET 10” in the transcript)
6. Create

**Do NOT add**: Individual Accounts / Identity / Areas / Roles.

---

## 2) Install NuGet packages (only what you need)
If the exam project doesn’t come with packages preinstalled, add these:

- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`

(Your professor mentioned these were the first ones used.)

---

## 3) Add connection string in `appsettings.json`
Open `appsettings.json` and add:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourLastNameDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Change `YourLastNameDB` to something unique.

---

## 4) Create your DbContext (simple `DbContext`, NOT Identity)
Create folder: **Data**
Create file: `Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using YourProjectName.Models;

namespace YourProjectName.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Course> Courses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data (only a couple rows; enough to prove it works)
            modelBuilder.Entity<Instructor>().HasData(
                new Instructor { InstructorId = 1, Name = "Dr. Smith" },
                new Instructor { InstructorId = 2, Name = "Prof. Lee" }
            );

            modelBuilder.Entity<Course>().HasData(
                new Course { CourseId = 1, Title = "Intro to MVC", Credits = 3, InstructorID = 1 },
                new Course { CourseId = 2, Title = "Databases", Credits = 4, InstructorID = 2 }
            );
        }
    }
}
```

Replace model names if your exam uses different entities.

---

## 5) Wire up EF Core in `Program.cs` (minimal, exam-safe)
Open `Program.cs` and make it look like this (adjust namespace/project name):

```csharp
using Microsoft.EntityFrameworkCore;
using YourProjectName.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

**Do not** add Identity, `AddDefaultIdentity`, `MapRazorPages`, or Areas unless explicitly asked.

---

## 6) Create the two related models (copy this pattern)
Create folder: **Models**

### Model 1: `Instructor`
```csharp
using System.ComponentModel.DataAnnotations;

namespace YourProjectName.Models
{
    public class Instructor
    {
        [Key]
        public int InstructorId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
```

### Model 2: `Course` (has foreign key → Instructor)
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourProjectName.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; }

        public int Credits { get; set; }

        // FK
        [Required]
        public int InstructorID { get; set; }

        [ForeignKey("InstructorID")]
        public Instructor? Instructor { get; set; }
    }
}
```

Notes:
- `[Required]` is normal exam-friendly validation.
- FK naming must be consistent: `InstructorID` ↔ `[ForeignKey("InstructorID")]`.

---

## 7) Create the database (Migrations)
Tools → **NuGet Package Manager** → **Package Manager Console**

Run:

```powershell
Add-Migration InitialCreate
Update-Database
```

If it fails, common reasons:
- Connection string typo
- Missing EF packages
- DbContext not registered in Program.cs
- Wrong default project selected in PMC

---

## 8) Choose your “dropdown strategy” (you only need ONE)
You need a way to pick the related model when creating/updating the main model.

### Pick ONE of these (recommended: ViewModel)
- ViewModel (cleanest)
- ViewBag (fast)
- ViewData (also fine)

This guide uses **ViewModel** because your project already uses that approach.

---

## 9) Build the ONE controller (2 actions/functions only)
Your professor: “one controller with two functionalities.”

Below is the **Index + Create** version (most common). If you get Edit/Delete instead, see later sections.

Create folder: **Controllers**
Create file: `Controllers/CourseController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YourProjectName.Data;
using YourProjectName.Models;
using YourProjectName.Models.ViewModels;

namespace YourProjectName.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _db;

        public CourseController(AppDbContext db)
        {
            _db = db;
        }

        // FUNCTION 1: Index
        public IActionResult Index()
        {
            var courses = _db.Courses.Include(c => c.Instructor).ToList();
            return View(courses);
        }

        // FUNCTION 2: Create (GET)
        [HttpGet]
        public IActionResult Create()
        {
            var instructors = _db.Instructors
                .ToList()
                .Select(i => new SelectListItem { Text = i.Name, Value = i.InstructorId.ToString() });

            var vm = new CourseWithInstructorsVM
            {
                Course = new Course(),
                Instructors = instructors
            };

            return View(vm);
        }

        // FUNCTION 2: Create (POST)
        [HttpPost]
        public IActionResult Create(CourseWithInstructorsVM vm)
        {
            if (ModelState.IsValid)
            {
                _db.Courses.Add(vm.Course);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            // IMPORTANT: if invalid, rebuild dropdown list before returning view
            vm.Instructors = _db.Instructors
                .ToList()
                .Select(i => new SelectListItem { Text = i.Name, Value = i.InstructorId.ToString() });

            return View(vm);
        }
    }
}
```

Why rebuild dropdown on invalid POST?
- Otherwise the ViewModel’s dropdown list is empty and the Create page breaks or shows empty options.

---

## 10) Create the ViewModel (only if you chose ViewModel approach)
Create folder: **Models/ViewModels**
Create file: `Models/ViewModels/CourseWithInstructorsVM.cs`

```csharp
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YourProjectName.Models.ViewModels
{
    public class CourseWithInstructorsVM
    {
        public Course Course { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Instructors { get; set; }
    }
}
```

---

## 11) Create the 2 Views (Index + Create)
Create folder path:
- `Views/Course/Index.cshtml`
- `Views/Course/Create.cshtml`

### View 1: `Views/Course/Index.cshtml`
```cshtml
@model List<Course>

<h1>Course List</h1>

<table class="table table-bordered table-striped">
    <thead>
        <tr>
            <th>Title</th>
            <th>Credits</th>
            <th>Instructor</th>
        </tr>
    </thead>

    <tbody>
    @foreach (var eachCourse in Model)
    {
        <tr>
            <td>@eachCourse.Title</td>
            <td>@eachCourse.Credits</td>
            <td>@(eachCourse.Instructor != null ? eachCourse.Instructor.Name : "")</td>
        </tr>
    }
    </tbody>
</table>

<div class="col-md-12 text-end">
    <a asp-controller="Course" asp-action="Create" class="btn btn-primary">Create New Course</a>
</div>
```

### View 2: `Views/Course/Create.cshtml`
```cshtml
@model CourseWithInstructorsVM

<h1>Add a New Course</h1>

<div class="border-bottom col-md-10 mx-auto">

    <form asp-action="Create">

        <div asp-validation-summary="All" class="text-danger"></div>

        <div class="form-group m-4">
            <label asp-for="Course.Title" class="control-label col-md-2"></label>
            <div>
                <input asp-for="Course.Title" class="form-control col-6" />
                <span asp-validation-for="Course.Title" class="text-danger"></span>
            </div>
        </div>

        <div class="form-group m-4">
            <label asp-for="Course.Credits" class="control-label col-md-2"></label>
            <div>
                <input asp-for="Course.Credits" class="form-control col-6" />
                <span asp-validation-for="Course.Credits" class="text-danger"></span>
            </div>
        </div>

        <div class="form-group m-4">
            <label asp-for="Course.InstructorID" class="control-label col-md-2"></label>
            <div>
                <select asp-for="Course.InstructorID" class="form-control" asp-items="@Model.Instructors">
                    <option disabled selected>Select an instructor</option>
                </select>
                <span asp-validation-for="Course.InstructorID" class="text-danger"></span>
            </div>
        </div>

        <div class="form-group m-4">
            <input type="submit" value="Create" class="form-control btn btn-outline-primary col-6" />
        </div>

    </form>

    @section Scripts
    {
        <partial name="_ValidationScriptsPartial" />
    }
</div>
```

This matches the style/pattern used in your class project: form groups, `asp-for`, field-level validation spans, and the scripts partial.

---

## 12) Verify it works (fast checklist)
- Run app
- Navigate to `/Course/Index`
- Confirm seeded courses appear (after Update-Database)
- Click Create
- Create a new course selecting an instructor from dropdown
- Submit → should redirect back to Index with the new row

---

## 13) If the exam asks for Edit instead of Create (swap “Function 2”)
If you’re assigned **Index + Edit**:

### Controller patterns (minimal)
```csharp
[HttpGet]
public IActionResult Edit(int id)
{
    var course = _db.Courses.Find(id);
    if (course == null) return NotFound();

    var instructors = _db.Instructors
        .ToList()
        .Select(i => new SelectListItem { Text = i.Name, Value = i.InstructorId.ToString() });

    var vm = new CourseWithInstructorsVM { Course = course, Instructors = instructors };
    return View(vm);
}

[HttpPost]
public IActionResult Edit(CourseWithInstructorsVM vm)
{
    if (ModelState.IsValid)
    {
        _db.Courses.Update(vm.Course);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    vm.Instructors = _db.Instructors
        .ToList()
        .Select(i => new SelectListItem { Text = i.Name, Value = i.InstructorId.ToString() });

    return View(vm);
}
```

### View
Edit view is basically the same as Create, just change title and submit button text.

---

## 14) If the exam asks for Delete instead of Create (swap “Function 2”)
If you’re assigned **Index + Delete**:

### Controller patterns (minimal)
```csharp
[HttpGet]
public IActionResult Delete(int id)
{
    var course = _db.Courses.Include(c => c.Instructor).FirstOrDefault(c => c.CourseId == id);
    if (course == null) return NotFound();
    return View(course);
}

[HttpPost]
[ActionName("Delete")]
public IActionResult DeletePost(int id)
{
    var course = _db.Courses.Find(id);
    if (course == null) return NotFound();

    _db.Courses.Remove(course);
    _db.SaveChanges();
    return RedirectToAction("Index");
}
```

### Delete view (simple confirm)
```cshtml
@model Course

<h1>Delete Course</h1>

<div>
    <h4>Are you sure you want to delete this?</h4>
    <hr />
    <p><b>Title:</b> @Model.Title</p>
    <p><b>Credits:</b> @Model.Credits</p>

    <form asp-action="Delete" method="post">
        <input type="hidden" asp-for="CourseId" />
        <input type="submit" value="Delete" class="btn btn-danger" />
        <a asp-action="Index" class="btn btn-secondary">Cancel</a>
    </form>
</div>
```

---

## 15) Most common exam mistakes (avoid these)
1. **Accidentally enabling Identity** (Individual Accounts) → too much complexity.
2. **Copying your class project’s advanced Program.cs** (Identity, Areas, extra mappings).
3. **DbContext is wrong type** (IdentityDbContext vs DbContext) when you don’t need Identity.
4. **Forgetting to register DbContext in Program.cs**.
5. **Forgetting migrations / Update-Database**.
6. **Dropdown empty on invalid POST** because you didn’t repopulate SelectList.
7. **Redirecting to an action that doesn’t exist** (ex: `RedirectToAction("Index")` without Index).
8. **Using custom server-side validation** when professor said not to.

---

## 16) “Translation table” (use any entity names)
Replace these pairs depending on the exam:
- Book ↔ Category
- Course ↔ Instructor
- Student ↔ Major
- Product ↔ Supplier

Pattern stays the same:
- Model A: lookup/reference table (Instructor)
- Model B: main table (Course) with FK to A
- Controller: typically for Model B (CourseController)
- Create/Edit view: dropdown for FK

---

## 17) Submission / packaging reminder (what professor said)
When you submit:
- Close the project
- Go to File Explorer for the project folder
- Go **one level up** (folder containing solution + project folders)
- Zip the whole folder (not just `.sln`)

---

## 18) Your “30-minute build” execution checklist
1. New MVC project (Auth None)
2. Add EF packages
3. Add connection string in appsettings.json
4. Add DbContext + DbSets + seed data
5. Program.cs: AddDbContext + routing
6. Models (2 related)
7. Add-Migration + Update-Database
8. Controller: Index + (Create/Edit/Delete)
9. Views: Index + (Create/Edit/Delete)
10. Run + verify

---

## Appendix: If you choose ViewBag instead of ViewModel (quick reference)
You can do this instead of ViewModel if you prefer speed.

### Controller Create GET
```csharp
public IActionResult Create()
{
    ViewBag.Instructors = _db.Instructors
        .ToList()
        .Select(i => new SelectListItem { Text = i.Name, Value = i.InstructorId.ToString() });

    return View(new Course());
}
```

### Create View
```cshtml
@model Course

<select asp-for="InstructorID" asp-items="ViewBag.Instructors" class="form-control">
    <option disabled selected>Select an instructor</option>
</select>
```

### Create POST
```csharp
[HttpPost]
public IActionResult Create(Course course)
{
    if (ModelState.IsValid)
    {
        _db.Courses.Add(course);
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    ViewBag.Instructors = _db.Instructors
        .ToList()
        .Select(i => new SelectListItem { Text = i.Name, Value = i.InstructorId.ToString() });

    return View(course);
}
```

Use **only one** approach (ViewModel OR ViewBag OR ViewData) in your exam project.
