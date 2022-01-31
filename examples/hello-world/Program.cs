using Pglet;
var page = await PgletClient.ConnectPage();
page.Add(new Text { Value = "Hello, world!" });