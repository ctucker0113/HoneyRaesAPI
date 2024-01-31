using HoneyRaesAPI.Models;
List<Customer> customers = new List<Customer>
{
    new Customer()
    {
        ID = 1,
        Name = "Harry Potter",
        Address = "4 Privet Drive, Little Whinging, Surrey"
    },

    new Customer()
    {
        ID = 2,
        Name = "Albus Dumbledore",
        Address = "Hogwarts Headmaster's Office, Hogwarts Castle, Scotland"
    },

    new Customer()
    {
        ID = 3,
        Name = "Ron Weasley",
        Address = "The Burrow, London, England"
    }
};

List<Employee> employees = new List<Employee>
{
    new Employee()
    {
        ID = 1,
        Name = "George Weasley",
        Specialty = "Blowing Stuff Up"
    },

    new Employee()
    {
        ID = 2,
        Name = "Fred Weasley",
        Specialty = "Practical Jokes"
    },

    new Employee()
    {
        ID = 3,
        Name = "Johnny Tight Lips",
        Specialty = "Not saying nothing"
    }
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket()
    {
        ID = 1,
        CustomerID = 1,
        Description = "Car done blowed up",
        Emergency = false,
    },

    new ServiceTicket()
    {
        ID = 2,
        CustomerID = 2,
        EmployeeID = 1,
        Description = "Wheel fell off",
        Emergency = false,
        DateCompleted = new DateTime (2024, 1, 15)
    },

    new ServiceTicket()
    {
        ID = 3,
        CustomerID = 2,
        EmployeeID = 1,
        Description = "Steering Wheel exploded",
        Emergency = false,
        DateCompleted = new DateTime(2024, 1, 17)
    },

    new ServiceTicket()
    {
        ID = 4,
        CustomerID = 3,
        EmployeeID = 2,
        Description = "Passenger side door eaten by raccoons",
        Emergency = true,
        DateCompleted = new DateTime (2022, 6, 7)
    },

    new ServiceTicket()
    {
        ID = 5,
        CustomerID = 3,
        Description = "Gas tank is empty",
        Emergency = false,
        DateCompleted = new DateTime (2022, 4, 10)
    },

    new ServiceTicket()
    {
        ID = 6,
        CustomerID = 3,
        Description = "KABOOM!",
        Emergency = true,
    },

    new ServiceTicket()
    {
        ID = 7,
        CustomerID = 2,
        Description = "Sharalandah",
        Emergency = false,
        EmployeeID = 1,
    }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


app.MapGet("/customers", () =>
{
    return customers;
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(e => e.ID == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerID == id).ToList();
    return Results.Ok(customer);
});

app.MapGet("/employees", () =>
{
    return employees;
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.ID == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeID == id).ToList();
    return Results.Ok(employee);
});

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.ID == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.ID == serviceTicket.EmployeeID);
    serviceTicket.Customer = customers.FirstOrDefault(e => e.ID == serviceTicket.CustomerID);
    return Results.Ok(serviceTicket);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.ID = serviceTickets.Max(st => st.ID) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.ID == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    serviceTickets.Remove(serviceTicket);
    return Results.Ok($"Service Ticket with ID {id} has been deleted.");
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.ID == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.ID)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

// UPDATE a service ticket to be COMPLETE
app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.ID == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});


//RETURN all service tickets that are incomplete and are emergencies
app.MapGet("/incompleteEmergencyServiceTickets", () =>
{
    var filteredServiceTickets = serviceTickets
        .Where(st => st.Emergency && st.DateCompleted == null)
        .ToList();

    return Results.Ok(filteredServiceTickets);
});

app.MapGet("/unassignedTickets", () =>
{
    var filteredServiceTickets = serviceTickets
    .Where(st => st.EmployeeID == null)
    .ToList();

    return Results.Ok(filteredServiceTickets);
});

app.MapGet("/inactiveCustomers", () =>
{
    var inactiveCustomers = customers.Where(customer =>
    {
        var customerTickets = serviceTickets.Where(st => st.CustomerID == customer.ID && st.DateCompleted.HasValue).ToList();

        if (!customerTickets.Any())
        {
            // No tickets, consider inactive
            return true;
        }

        var lastTicketDate = customerTickets.Max(st => st.DateCompleted.Value);
        return (DateTime.Today - lastTicketDate).Days > 365;
    }).ToList();

    return Results.Ok(inactiveCustomers);
});


app.MapGet("/availableEmployees", () =>
{
    // Filter out tickets that have been completed.
    var incompleteTickets = serviceTickets.Where(st => st.DateCompleted == null);

    // Create a new list of all employees attached to the unassigned tickets.
    var assignedEmployees = incompleteTickets
    // Filter out any tickets that don't have an employee assigned
                            .Where(st => st.EmployeeID.HasValue)
    // Pull out the Employee ID values into a new list.
                            .Select(st => st.EmployeeID.Value)
    // Prevent any duplicate Employee ID's from making it into the list.
                            .Distinct();

    var availableEmployees = employees
    // If the employee is NOT in the assignedEmployees array (and therefore free), add them to this list.
                            .Where(e => !assignedEmployees.Contains(e.ID))
                            .ToList();

    return Results.Ok(availableEmployees);
});

app.MapGet("/allCustomersOfAnEmployee/{employeeID}", (int employeeID) =>
{
    // Find all the service tickets attached to a single employeeID
    var allCustomerIDsOfAnEmployee = serviceTickets
                                   .Where(e => e.EmployeeID == employeeID)
    // Pull the customerID's on those tickets. 
                                   .Select(e => e.CustomerID)
    // Filter out duplicates.
                                   .Distinct();


    var associatedCustomers = customers
    // Find all the objects in the customers list that contain the ID numbers in the list created above.
                              .Where(e => allCustomerIDsOfAnEmployee.Contains(e.ID))
                              .ToList();

    return Results.Ok(associatedCustomers);

});

app.MapGet("/employeeOfTheMonth", () =>
{
    // Define the start and end dates for the last month
    var endDate = DateTime.Today;
    var startDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-1);

    // Filter the service tickets down to just tickets that have been completed in the last month
    var lastMonthTickets = serviceTickets.Where(st => st.DateCompleted.HasValue && st.DateCompleted.Value >= startDate && st.DateCompleted.Value < endDate);

    // Count the number of tickets per employee
    var employeeTicketCounts = lastMonthTickets
    // Establishes EmployeeID as the group key.
                                .GroupBy(st => st.EmployeeID)
                                .Select(group => new
                                {
                                    EmployeeID = group.Key,
                                    TicketCount = group.Count()
                                })
                                .ToList();

    // Find the employee with the most tickets completed
    var maxTickets = employeeTicketCounts.MaxBy(e => e.TicketCount);

    // Fetch the details of this employee
    var employeeOfTheMonth = employees.FirstOrDefault(e => e.ID == maxTickets.EmployeeID);

    return employeeOfTheMonth != null ? Results.Ok(employeeOfTheMonth) : Results.NotFound("No employee of the month found.");
});

app.MapGet("/completedTicketsList", () =>
{
    // Filter out the incomplete tickets
    var completedTickets = serviceTickets.Where(e => e.DateCompleted != null).OrderBy(e => e.DateCompleted);
    return completedTickets;

});

app.MapGet("/prioritizedTickets", () =>
{
    var prioritizedTickets = serviceTickets.Where(e => e.DateCompleted == null).OrderByDescending(e => e.Emergency).ThenBy(e => e.EmployeeID);

    return prioritizedTickets;
});

app.Run();