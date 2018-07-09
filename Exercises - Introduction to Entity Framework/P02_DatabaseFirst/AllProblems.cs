namespace P02_DatabaseFirst
{
    using Data;
    using Data.Models;
    using System;
    using System.Linq;

    public class AllProblems
    {
        public static void Main()
        {
            using (var db = new SoftUniContext())
            {
                //P03_EmployeesFullInformation(db);
                //P04_EmployeesWithSalaryOver50000(db);
                //P05_EmployeesFromResearchAndDevelopment(db);
                //P06_AddingANewAddressAndUpdatingEmployee(db);
                //P07_EmployeesAndProjects(db);
                //P08_AddressByTown(db);
                //P09_Employee147(db);
                //P10_DepartmentsWithMoreThanFiveEmployees(db);
                //P11_FindLatest10Projects(db);
                //P12_IncreaseSalaries(db);
                //P13_FindEmployeesByFirstNameStartingWithSa(db);
                //P14_DeleteProjectById(db);
                //P15_RemoveTowns(db);
            }
        }

        private static void P15_RemoveTowns(SoftUniContext db)
        {
            while (true)
            {
                Console.Write("Please enter town name: ");
                var townName = Console.ReadLine();

                var affectedEmployees = db.Employees
                    .Where(e => e.Address.Town.Name == townName);

                foreach (var affectedEmployee in affectedEmployees)
                {
                    affectedEmployee.AddressId = null;
                }

                var adresses = affectedEmployees
                    .Select(a => a.Address);
                db.Addresses.RemoveRange(adresses);

                if (adresses.Any())
                {
                    var town = adresses
                        .Select(x => x.Town)
                        .First();
                    db.Towns.Remove(town);
                }

                var deletedAddresses = adresses.Count();

                db.SaveChanges();

                Console.WriteLine(deletedAddresses == 1
                    ? $"{deletedAddresses} address in {townName} was deleted"
                    : $"{deletedAddresses} addresses in {townName} were deleted");

                Console.Write("Would you like to continue ? (Yes/No): ");
                var answer = Console.ReadLine();
                if (answer.ToLower().Equals("yes"))
                {
                    continue;
                }

                if (answer.ToLower().Equals("no"))
                {
                    break;
                }

                Console.WriteLine("Invalid input");
            }
        }

        private static void P14_DeleteProjectById(SoftUniContext db)
        {
            var projectId = 2;
            var employeesProjects = db.EmployeesProjects
                .Where(e => e.ProjectId == projectId);
            foreach (var employeesProject in employeesProjects)
            {
                db.EmployeesProjects.Remove(employeesProject);
            }

            var projectToRemove = db.Projects
                .Find(projectId);
            db.Projects.Remove(projectToRemove);

            db.SaveChanges();

            var projects = db.Projects
                .Take(10)
                .Select(p => p.Name);

            foreach (var project in projects)
            {
                Console.WriteLine($"{project}");
            }
        }

        private static void P13_FindEmployeesByFirstNameStartingWithSa(SoftUniContext db)
        {
            var employees = db.Employees
                                .Where(e => e.FirstName.StartsWith("Sa"))
                                .OrderBy(e => e.FirstName)
                                .ThenBy(e => e.LastName)
                                .Select(e => new
                                {
                                    Name = $"{e.FirstName} {e.LastName}",
                                    e.JobTitle,
                                    e.Salary
                                });

            foreach (var employee in employees)
            {
                Console.WriteLine($"{employee.Name} - {employee.JobTitle} - (${employee.Salary:f2})");
            }
        }

        private static void P12_IncreaseSalaries(SoftUniContext db)
        {
            var employees = db.Employees
                                .Where(e => e.Department.Name == "Engineering" || e.Department.Name == "Tool Design" ||
                                            e.Department.Name == "Marketing" || e.Department.Name == "Information Services")
                                .ToList();

            foreach (var employee in employees)
            {
                employee.Salary *= 1.12m;
            }

            //db.SaveChanges();

            var employeesAfterIncreasedSalary = employees
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new
                {
                    Name = $"{e.FirstName} {e.LastName}",
                    e.Salary
                });

            foreach (var employee in employeesAfterIncreasedSalary)
            {
                Console.WriteLine($"{employee.Name} (${employee.Salary:f2})");
            }
        }

        private static void P11_FindLatest10Projects(SoftUniContext db)
        {
            var projects = db.Projects
                                .OrderByDescending(p => p.StartDate)
                                .Select(p => new
                                {
                                    p.Name,
                                    p.Description,
                                    p.StartDate
                                })
                                .Take(10)
                                .OrderBy(p => p.Name);

            foreach (var project in projects)
            {
                Console.WriteLine($"{project.Name}");
                Console.WriteLine($"{project.Description}");
                Console.WriteLine($"{project.StartDate:M/d/yyyy h:mm:ss tt}");
            }
        }

        private static void P10_DepartmentsWithMoreThanFiveEmployees(SoftUniContext db)
        {
            var departments = db.Departments
                                .Where(d => d.Employees.Count > 5)
                                .OrderBy(d => d.Employees.Count)
                                .ThenBy(d => d.Name)
                                .Select(d => new
                                {
                                    DepartmentName = d.Name,
                                    ManagerName = $"{d.Manager.FirstName} {d.Manager.LastName}",
                                    Employees = d.Employees.Select(e => new
                                    {
                                        EmployeeFirstName = e.FirstName,
                                        EmployeeLastName = e.LastName,
                                        e.JobTitle,
                                    })
                                    .OrderBy(e => e.EmployeeFirstName)
                                    .ThenBy(e => e.EmployeeLastName)
                                })
                                .ToList();

            foreach (var department in departments)
            {
                Console.WriteLine($"{department.DepartmentName} - {department.ManagerName}");

                foreach (var employee in department.Employees)
                {
                    Console.WriteLine($"{employee.EmployeeFirstName} {employee.EmployeeLastName} - {employee.JobTitle}");
                }

                Console.WriteLine("----------");
            }
        }

        private static void P09_Employee147(SoftUniContext db)
        {
            var employee = db.Employees
                                .Where(e => e.EmployeeId == 147)
                                .Select(e => new
                                {
                                    Name = $"{e.FirstName} {e.LastName}",
                                    e.JobTitle,
                                    Projects = e.EmployeesProjects.Select(p => new
                                    {
                                        p.Project.Name,
                                    })
                                });

            foreach (var emp in employee)
            {
                Console.WriteLine($"{emp.Name} - {emp.JobTitle}");

                foreach (var project in emp.Projects.OrderBy(p => p.Name))
                {
                    Console.WriteLine($"{project.Name}");
                }
            }
        }

        private static void P08_AddressByTown(SoftUniContext db)
        {
            var addresses = db.Addresses
                                .OrderByDescending(e => e.Employees.Count)
                                .ThenBy(t => t.Town.Name)
                                .ThenBy(a => a.AddressText)
                                .Take(10)
                                .Select(a => new
                                {
                                    Text = a.AddressText,
                                    TownName = a.Town.Name,
                                    a.Employees.Count
                                });

            foreach (var address in addresses)
            {
                Console.WriteLine($"{address.Text}, {address.TownName} - {address.Count} employees");
            }
        }

        private static void P07_EmployeesAndProjects(SoftUniContext db)
        {
            var employees = db.Employees
                                    .Where(e =>
                                        e.EmployeesProjects.Any(p =>
                                        p.Project.StartDate.Year >= 2001 &&
                                        p.Project.StartDate.Year <= 2003))
                                .Take(30)
                                .Select(e => new
                                {
                                    PersonName = $"{e.FirstName} {e.LastName}",
                                    ManagerName = $"{e.Manager.FirstName} {e.Manager.LastName}",
                                    Projects = e.EmployeesProjects
                                        .Select(ep => new
                                        {
                                            ep.Project.Name,
                                            ep.Project.StartDate,
                                            ep.Project.EndDate
                                        })
                                })
                                .ToList();

            foreach (var employee in employees)
            {
                Console.WriteLine($"{employee.PersonName} - Manager: {employee.ManagerName}");

                foreach (var project in employee.Projects)
                {
                    Console.Write($"--{project.Name} - {project.StartDate:M/d/yyyy h:mm:ss tt} - ");
                    Console.WriteLine(project.EndDate == null ? "not finished" : $"{project.EndDate:M/d/yyyy h:mm:ss tt}");
                }

            }
        }

        private static void P06_AddingANewAddressAndUpdatingEmployee(SoftUniContext db)
        {
            var address = new Address
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };
            db.Addresses.Add(address);

            var wantedEmployee = db.Employees
                .FirstOrDefault(e => e.LastName == "Nakov");

            if (wantedEmployee != null)
            {
                wantedEmployee.Address = address;
                db.SaveChanges();
            }

            var result = db.Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => e.Address.AddressText);

            foreach (var res in result)
            {
                Console.WriteLine(res);
            }
        }

        private static void P05_EmployeesFromResearchAndDevelopment(SoftUniContext db)
        {
            var employees = db.Employees
                .Where(d => d.Department.Name == "Research and Development")
                .Select(e => new
                {
                    PersonName = $"{e.FirstName} {e.LastName}",
                    Department = e.Department.Name,
                    e.Salary
                })
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.PersonName)
                .ToList();

            foreach (var employee in employees)
            {
                Console.WriteLine($"{employee.PersonName} from {employee.Department} - ${employee.Salary:f2}");
            }
        }

        private static void P04_EmployeesWithSalaryOver50000(SoftUniContext db)
        {
            var employees = db.Employees
                                .OrderBy(e => e.FirstName)
                                .Where(e => e.Salary > 50000)
                                .Select(e => e.FirstName)
                                .ToList();

            foreach (var employee in employees)
            {
                Console.WriteLine($"{employee}");
            }
        }

        private static void P03_EmployeesFullInformation(SoftUniContext db)
        {
            var employees = db.Employees
                                .OrderBy(e => e.EmployeeId)
                                .Select(e => new
                                {
                                    Name = $"{e.FirstName} {e.LastName} {e.MiddleName}",
                                    e.JobTitle,
                                    e.Salary
                                })
                                .ToList();

            foreach (var employee in employees)
            {
                Console.WriteLine($"{employee.Name} {employee.JobTitle} {employee.Salary:f2}");
            }
        }
    }
}
