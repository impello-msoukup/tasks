namespace server.tests;

public class OverallTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public OverallTests(WebApplicationFactory<Program> factory, ITestOutputHelper output) {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async void Test1()
    {
        var client = _factory.CreateClient();
        var initTaskName = "Test";
        var updatedTaskName = "Test update";
        var jsonOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            };

        // Create task
        Task reqData = new Task();
        reqData.Name = null;
        reqData.Priority = 1;
        reqData.Status = 0;

        // Post task without name
        var payloadData = JsonSerializer.Serialize(reqData);
        var content = new StringContent(payloadData, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/v1/Tasks", content);
        Assert.Equal(400, (int)response.StatusCode);

        // Post valid task
        reqData.Name = initTaskName;
        payloadData = JsonSerializer.Serialize(reqData);
        content = new StringContent(payloadData, Encoding.UTF8, "application/json");
        response = await client.PostAsync("/v1/Tasks", content);
        response.EnsureSuccessStatusCode();

        // Get tasks
        response = await client.GetAsync("/v1/Tasks");
        response.EnsureSuccessStatusCode();
        string responseString = await response.Content.ReadAsStringAsync();
        List<Task>? tasks = JsonSerializer.Deserialize<List<Task>>(responseString, jsonOptions);
        Assert.NotEmpty(tasks);
        var task = tasks?.Find(x => x.Name == initTaskName);
        Assert.Equal(initTaskName, task?.Name);

        // Put change of task name
        reqData.Name = updatedTaskName;
        payloadData = JsonSerializer.Serialize(reqData);
        content = new StringContent(payloadData, Encoding.UTF8, "application/json");
        response = await client.PutAsync($"/v1/Tasks/{task?.Identifier}", content);
        response.EnsureSuccessStatusCode();

        // Get tasks
        response = await client.GetAsync("/v1/Tasks");
        response.EnsureSuccessStatusCode();
        responseString = await response.Content.ReadAsStringAsync();
        tasks = JsonSerializer.Deserialize<List<Task>>(responseString, jsonOptions);
        Assert.NotEmpty(tasks);
        task = tasks?.Find(x => x.Name == updatedTaskName);
        Assert.Equal(updatedTaskName, task?.Name);

        // Delete of uncompleted task
        response = await client.DeleteAsync($"/v1/Tasks/{task?.Identifier}");
        Assert.Equal(400, (int)response.StatusCode);

        // Put change of task status
        reqData.Status = Status.Completed;
        payloadData = JsonSerializer.Serialize(reqData);
        content = new StringContent(payloadData, Encoding.UTF8, "application/json");
        response = await client.PutAsync($"/v1/Tasks/{task?.Identifier}", content);
        response.EnsureSuccessStatusCode();

        // Delete of completed task
        response = await client.DeleteAsync($"/v1/Tasks/{task?.Identifier}");
        response.EnsureSuccessStatusCode();

        // Get tasks
        response = await client.GetAsync("/v1/Tasks");
        response.EnsureSuccessStatusCode();
        responseString = await response.Content.ReadAsStringAsync();
        tasks = JsonSerializer.Deserialize<List<Task>>(responseString, jsonOptions);
        Assert.Empty(tasks);
    }
}