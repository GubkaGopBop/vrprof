"""Run the Java CLI on real Unity exports, negative inputs and the C# reference."""
from pathlib import Path
import copy
import json
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
MODULE = ROOT / 'JavaAnalyzer'
RESULTS = MODULE / 'results'
RESULTS.mkdir(exist_ok=True)
checks = []

def execute(arguments):
    return subprocess.run(arguments, cwd=ROOT, capture_output=True, text=True, encoding='utf-8', errors='replace')

def check(name, passed, detail=''):
    checks.append(dict(name=name, passed=bool(passed), detail=detail))

jar = MODULE / 'build/vrprof-analyzer.jar'
for name in ['completed', 'incomplete']:
    source = MODULE / f'examples/{name}.json'
    proc = execute(['java', '-Dfile.encoding=UTF-8', '-jar', str(jar), str(source), str(RESULTS/name)])
    check(f'{name}: Java CLI exit 0', proc.returncode == 0, proc.stderr.strip())
    if proc.returncode:
        continue
    actual = json.loads(proc.stdout)
    result = actual['result']
    expected = (9, 1, 1, True) if name == 'completed' else (4, 0, 1, False)
    check(f'{name}: independently expected counters',
          (result['completedSteps'], result['hints'], result['errors'], actual['completed']) == expected)
    cross = execute(['dotnet', 'run', '--project', str(ROOT/'Verification/CrossCheck'), '--',
                     str(source), str(RESULTS/name/'analysis.json')])
    (RESULTS/f'crosscheck-{name}.txt').write_text(cross.stdout + cross.stderr, encoding='utf-8')
    check(f'{name}: Java == C# == Unity saved summary', cross.returncode == 0, cross.stdout.strip())

baseline = json.loads((MODULE/'examples/completed.json').read_text(encoding='utf-8-sig'))
mutations = []
def mutation(name, change, expected_code=1, assertion=None):
    sample = copy.deepcopy(baseline)
    change(sample)
    mutations.append((name, json.dumps(sample, ensure_ascii=False), expected_code, assertion))

mutation('Unknown exercise', lambda x: x.update(ExerciseId='other'))
mutation('Missing Events', lambda x: x.pop('Events'))
mutation('Null Events', lambda x: x.update(Events=None))
mutation('Events is not array', lambda x: x.update(Events={}))
mutation('Number gap', lambda x: x['Events'][0].update(Number=2))
mutation('Unknown event kind', lambda x: x['Events'][0].update(Kind='unknown'))
mutation('Invalid difficulty', lambda x: x.update(Difficulty=0))
mutation('Invalid interest', lambda x: x.update(Interest=6))
mutation('Fractional rating', lambda x: x.update(Difficulty=2.5))
mutation('String instead of number', lambda x: x.update(Difficulty='2'))
mutation('String instead of boolean', lambda x: x['Events'][0].update(Success='true'))
mutation('Missing action object', lambda x: x['Events'][0].pop('ObjectId'))
mutation('Null action code', lambda x: x['Events'][0].update(StepCode=None))
mutation('Blank pupil code', lambda x: x.update(PupilCode=' '))
mutation('Empty log remains incomplete', lambda x: x.update(Events=[]), 0,
         lambda r: not r['completed'] and r['result']['completedSteps']==0)
mutation('Untrusted stored Summary ignored', lambda x: x.update(Summary='FALSE_RESULT'), 0,
         lambda r: 'FALSE_RESULT' not in r['summary'] and r['result']['completedSteps']==9)
mutation('Failed first action cannot complete', lambda x: x['Events'][0].update(Success=False), 0,
         lambda r: not r['completed'] and r['result']['completedSteps']==0)
mutation('Action after completion counted as error', lambda x: x['Events'].append(
    dict(Number=len(x['Events'])+1,Kind='action',StepCode='RUN_TEST',ObjectId='memory_test',Success=True)),0,
    lambda r:r['completed'] and r['result']['errors']==2)
mutations.append(('Malformed JSON', '{', 1, None))
mutations.append(('Trailing JSON document', json.dumps(baseline)+' {}', 1, None))

with tempfile.TemporaryDirectory(prefix='vrprof-check-') as directory:
    source = Path(directory)/'input.json'
    for name, content, expected_code, assertion in mutations:
        source.write_text(content,encoding='utf-8')
        proc=execute(['java','-Dfile.encoding=UTF-8','-jar',str(jar),str(source)])
        ok=proc.returncode==expected_code
        if ok and assertion:
            ok=assertion(json.loads(proc.stdout))
        check(name,ok,proc.stderr.strip())
    proc=execute(['java','-jar',str(jar),str(Path(directory)/'absent.json')])
    check('Missing file',proc.returncode==1,proc.stderr.strip())
proc=execute(['java','-jar',str(jar)])
check('Missing CLI arguments',proc.returncode==2,proc.stderr.strip())
report=dict(total=len(checks),passed=sum(x['passed'] for x in checks),failed=sum(not x['passed'] for x in checks),checks=checks)
(RESULTS/'verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='checks'},ensure_ascii=False))
for entry in checks:
    if not entry['passed']: print('FAILED:',entry['name'],entry['detail'])
raise SystemExit(0 if report['failed']==0 else 1)
