using System.Text.Json;
using System.Text.Encodings.Web;
using VrProf;

var steps = TrialLogic.CreateRamReplacementSteps();
var checks = new List<object>();
int failed = 0;
List<TrialEvent> Prefix(int length) => steps.Take(length).Select((s,i)=>new TrialEvent(i+1,"action",s.Code,s.ObjectId)).ToList();
void Check(string name, List<TrialEvent> events, int done, int hints, int errors, bool completed) {
    var actual = TrialLogic.Evaluate(steps, events);
    bool passed = actual.CompletedSteps==done && actual.Hints==hints && actual.Errors==errors && actual.Completed==completed;
    if (!passed) failed++;
    checks.Add(new { name, passed, inputEvents=events,
        expected=new {completedSteps=done,hints,errors,completed},
        actual=new {completedSteps=actual.CompletedSteps,totalSteps=actual.TotalSteps,hints=actual.Hints,errors=actual.Errors,completed=actual.Completed,errorDetails=actual.ErrorDetails} });
}
var wrong = Prefix(4);
wrong.Add(new TrialEvent(5,"hint"));
wrong.Add(new TrialEvent(6,"action","SELECT_FAULTY","ram_slot_a"));
Check("same_action_wrong_object",wrong,4,1,1,false);
var corrected = new List<TrialEvent>(wrong) { new TrialEvent(7,"action","SELECT_FAULTY","ram_slot_b") };
Check("correct_object_after_error",corrected,5,1,1,false);
var extra = Prefix(9);
extra.Add(new TrialEvent(10,"action","RUN_TEST","memory_test"));
Check("action_after_completion",extra,9,0,1,true);
var unsuccessful = new List<TrialEvent> {new TrialEvent(1,"action","READ_REPORT","diagnostic_report",false)};
Check("matching_but_unsuccessful",unsuccessful,0,0,1,false);
var finalHint = Prefix(9);
finalHint.Add(new TrialEvent(10,"hint"));
Check("hint_after_completion",finalHint,9,1,0,true);
var json = JsonSerializer.Serialize(new {total=checks.Count,failed,checks},new JsonSerializerOptions {
    WriteIndented=true,IncludeFields=true,Encoder=JavaScriptEncoder.UnsafeRelaxedJsonEscaping});
if (args.Length==1) File.WriteAllText(args[0],json+"\n",System.Text.Encoding.UTF8);
Console.WriteLine(json);
return failed==0 ? 0 : 1;
