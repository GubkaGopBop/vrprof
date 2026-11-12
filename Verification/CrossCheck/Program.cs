using System.Text.Json;
using VrProf;

if (args.Length != 2) return 2;
using var doc = JsonDocument.Parse(File.ReadAllText(args[0]));
var root = doc.RootElement;
var events = new List<TrialEvent>();
foreach (var element in root.GetProperty("Events").EnumerateArray())
    events.Add(new TrialEvent(element.GetProperty("Number").GetInt32(),element.GetProperty("Kind").GetString(),
        element.GetProperty("StepCode").GetString(),element.GetProperty("ObjectId").GetString(),element.GetProperty("Success").GetBoolean()));
var result = TrialLogic.Evaluate(TrialLogic.CreateRamReplacementSteps(),events);
var summary = TrialLogic.BuildSummary("Замена модуля оперативной памяти",root.GetProperty("PupilCode").GetString(),result,
    root.GetProperty("Difficulty").GetInt32(),root.GetProperty("Interest").GetInt32(),
    root.GetProperty("StudentFeedback").GetString(),root.GetProperty("MentorFeedback").GetString());
using var java = JsonDocument.Parse(File.ReadAllText(args[1]));
var jr = java.RootElement.GetProperty("result");
bool ok = jr.GetProperty("completedSteps").GetInt32()==result.CompletedSteps
    && jr.GetProperty("totalSteps").GetInt32()==result.TotalSteps
    && jr.GetProperty("hints").GetInt32()==result.Hints && jr.GetProperty("errors").GetInt32()==result.Errors
    && java.RootElement.GetProperty("completed").GetBoolean()==result.Completed
    && java.RootElement.GetProperty("summary").GetString().Replace("\r\n","\n")==summary.Replace("\r\n","\n")
    && root.GetProperty("Summary").GetString().Replace("\r\n","\n")==summary.Replace("\r\n","\n")
    && jr.GetProperty("errorDetails").EnumerateArray().Select(e=>e.GetString()).SequenceEqual(result.ErrorDetails);
Console.WriteLine(JsonSerializer.Serialize(new {input=Path.GetFileName(args[0]),matched=ok,completed=result.Completed,
    completedSteps=result.CompletedSteps,totalSteps=result.TotalSteps,hints=result.Hints,errors=result.Errors,summaryMatched=ok}));
return ok ? 0 : 1;
