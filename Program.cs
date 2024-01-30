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
    }
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket()
    {
        ID = 1,
        CustomerID = 1,
        Description = "Car done blowed up",
        Emergency = true,
    },

    new ServiceTicket()
    {
        ID = 2,
        CustomerID = 2,
        EmployeeID = 1,
        Description = "Wheel fell off",
        Emergency = false,
        DateCompleted = new DateTime (2023, 10, 15)
    },

    new ServiceTicket()
    {
        ID = 3,
        CustomerID = 2,
        EmployeeID = 1,
        Description = "Steering Wheel exploded",
        Emergency = false,
    },

    new ServiceTicket()
    {
        ID = 4,
        CustomerID = 3,
        EmployeeID = 2,
        Description = "Passenger side door eaten by raccoons",
        Emergency = true,
        DateCompleted = new DateTime (2023, 6, 7)
    },

    new ServiceTicket()
    {
        ID = 5,
        CustomerID = 3,
        Description = "Gas tank is empty",
        Emergency = false,
        DateCompleted = new DateTime (2023, 4, 10)
    },
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

app.Run();
