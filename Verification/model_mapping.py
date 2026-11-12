"""Produce and validate a ten-entity logical view of a real Unity export.

This is a logical data example; it does not create a physical database.
"""
from pathlib import Path
import json
import re

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/data-model'
OUT.mkdir(parents=True,exist_ok=True)
source=ROOT/'JavaAnalyzer/examples/completed.json'
raw=json.loads(source.read_text(encoding='utf-8-sig'))
analysis=json.loads((ROOT/'JavaAnalyzer/results/completed/analysis.json').read_text(encoding='utf-8'))
source_steps=re.findall(r'new TrialStep\("([^"]+)", "([^"]+)"\)',(ROOT/'Assets/Scripts/TrialLogic.cs').read_text(encoding='utf-8'))
assert len(source_steps)==9
exercise_id=raw['ExerciseId']
object_codes=list(dict.fromkeys([o for _,o in source_steps]+[e['ObjectId'] for e in raw['Events'] if e['Kind']=='action']))
labels={
 'diagnostic_report':('Результат модельной диагностики','document'),
 'pc':('Виртуальный персональный компьютер','computer'),
 'power_cable':('Кабель питания компьютера','cable'),
 'case':('Корпус компьютера','case'),
 'ram_slot_a':('Модуль памяти в слоте A','ram_module'),
 'ram_slot_b':('Модуль памяти в слоте B','ram_module'),
 'memory_test':('Средство модельной проверки памяти','tool')}
object_ids={code:f'object-{i+1}' for i,code in enumerate(object_codes)}
attempt_id=raw['AttemptId']
student_id='participant-student-001';mentor_id='participant-mentor-001'
step_titles=['Прочитать диагностику','Выключить ПК','Отсоединить питание','Открыть корпус',
 'Выбрать неисправный модуль','Извлечь модуль','Установить исправный модуль','Закрыть корпус','Повторить модельную проверку']
model={
 'roles':[{'id':'role-student','name':'Школьник'},{'id':'role-mentor','name':'Наставник'}],
 'participants':[
  {'id':student_id,'role_id':'role-student','code':raw['PupilCode'],'display_label':'Учебный участник '+raw['PupilCode']},
  {'id':mentor_id,'role_id':'role-mentor','code':raw['MentorCode'],'display_label':'Учебный наставник '+raw['MentorCode']}],
 'exercises':[{'id':exercise_id,'title':'Замена модуля оперативной памяти','profession':'Обслуживание ПК','version':1}],
 'training_objects':[dict(id=object_ids[code],exercise_id=exercise_id,code=code,label=labels[code][0],object_type=labels[code][1]) for code in object_codes],
 'steps':[{'id':f'step-{i+1}','exercise_id':exercise_id,'position':i+1,'title':step_titles[i]} for i,(code,_) in enumerate(source_steps)],
 'rules':[{'id':f'rule-{i+1}','step_id':f'step-{i+1}','expected_action':code,'expected_object_id':object_ids[obj],'require_success':True} for i,(code,obj) in enumerate(source_steps)],
 'attempts':[{'id':attempt_id,'exercise_id':exercise_id,'pupil_id':student_id,'mentor_id':mentor_id,'status':'completed' if analysis['completed'] else 'incomplete'}],
 'events':[{'id':f'{attempt_id}-event-{e["Number"]}','attempt_id':attempt_id,'number':e['Number'],'kind':e['Kind'],
            'action_code':e['StepCode'] if e['Kind']=='action' else None,
            'actual_object_id':object_ids[e['ObjectId']] if e['Kind']=='action' else None,
            'success':e['Success'] if e['Kind']=='action' else None} for e in raw['Events']],
 'feedback':[{'id':attempt_id+'-student-feedback','attempt_id':attempt_id,'author_id':student_id,'text':raw['StudentFeedback'],'difficulty':raw['Difficulty'],'interest':raw['Interest']},
             {'id':attempt_id+'-mentor-feedback','attempt_id':attempt_id,'author_id':mentor_id,'text':raw['MentorFeedback'],'difficulty':None,'interest':None}],
 'results':[{'id':attempt_id+'-result','attempt_id':attempt_id,
    'completed_steps':analysis['result']['completedSteps'],'total_steps':analysis['result']['totalSteps'],
    'hints':analysis['result']['hints'],'errors':analysis['result']['errors'],
    'error_details':analysis['result']['errorDetails'],'summary':analysis['summary']}]
}
assert len(model)==10
relations=[('participants','role_id','roles'),('training_objects','exercise_id','exercises'),
 ('steps','exercise_id','exercises'),('rules','step_id','steps'),('rules','expected_object_id','training_objects'),
 ('attempts','exercise_id','exercises'),('attempts','pupil_id','participants'),('attempts','mentor_id','participants'),
 ('events','attempt_id','attempts'),('events','actual_object_id','training_objects'),
 ('feedback','attempt_id','attempts'),('feedback','author_id','participants'),('results','attempt_id','attempts')]
checks=[]
for table,rows in model.items():
 assert len({r['id'] for r in rows})==len(rows)
for table,field,target in relations:
 ids={r['id'] for r in model[target]}
 assert all(r[field] is None or r[field] in ids for r in model[table])
 checks.append(f'{table}.{field} -> {target}.id')
assert len({r['step_id'] for r in model['rules']})==len(model['steps'])
assert len({(r['attempt_id'],r['author_id']) for r in model['feedback']})==len(model['feedback'])
assert all(e['number']==i+1 for i,e in enumerate(model['events']))
participants={r['id']:r for r in model['participants']}
assert participants[model['attempts'][0]['pupil_id']]['role_id']=='role-student'
assert participants[model['attempts'][0]['mentor_id']]['role_id']=='role-mentor'
assert all(f['author_id'] in {student_id,mentor_id} for f in model['feedback'])
assert all(f['difficulty'] is None and f['interest'] is None if f['author_id']==mentor_id
           else 1<=f['difficulty']<=5 and 1<=f['interest']<=5 for f in model['feedback'])
objects={r['id']:r for r in model['training_objects']}
rule=model['rules'][4];event=model['events'][5]
assert event['action_code']==rule['expected_action']=='SELECT_FAULTY'
assert event['actual_object_id']!=rule['expected_object_id']
assert analysis['result']['errors']==1
trace={'attempt_id':attempt_id,'student_id':student_id,'mentor_id':mentor_id,'event_number':6,
 'action':event['action_code'],'actual_object':objects[event['actual_object_id']],
 'expected_object':objects[rule['expected_object_id']],'rule_id':rule['id'],
 'result_id':model['results'][0]['id'],'mentor_feedback_id':model['feedback'][1]['id']}
schema={name:{'fields':list(rows[0]),'primary_key':'id'} for name,rows in model.items()}
for filename,data in [('ten-entity-example.json',model),('schema.json',schema),('trace.json',trace),
 ('mapping-check.json',dict(entities=len(model),foreign_key_relations_checked=len(relations),passed=True,relations=checks))]:
 (OUT/filename).write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(entities=len(model),foreign_key_relations_checked=len(relations),passed=True)))
