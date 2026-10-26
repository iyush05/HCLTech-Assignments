const $ = id => document.getElementById(id);
const make = (tag, text='', className='') => {const node=document.createElement(tag);node.textContent=text;node.className=className;return node;};
const date = value => new Date(value).toLocaleDateString(undefined,{month:'short',day:'numeric',year:'numeric'});
let busy=false;
function message(text,error=false){$('status').textContent=text;$('status').classList.toggle('error',error);}
async function api(path,method='GET',body){
 let response;
 try {response=await fetch(path,{method,headers:{'Content-Type':'application/json'},...(body===undefined?{}:{body:JSON.stringify(body)})});}
 catch {throw new Error('Cannot reach the app. Check that its server is running, then try again.');}
 if(response.status===204)return null;
 let data;try{data=await response.json();}catch{throw new Error('The request could not be processed. Check your values and try again.');}
 if(!response.ok)throw new Error(data.error||'The request could not be completed. Please try again.');
 return data;
}
async function run(action){
 if(busy)return;busy=true;
 const controls=[...document.querySelectorAll('button,input,select')].map(node=>[node,node.disabled]);
 controls.forEach(([node])=>node.disabled=true);
 document.querySelector('main').setAttribute('aria-busy','true');
 try{await action();}catch(error){message(error.message,true);}
 finally{busy=false;controls.forEach(([node,disabled])=>{if(node.isConnected)node.disabled=disabled;});document.querySelector('main').removeAttribute('aria-busy');}
}
function button(label,handler,className='icon-button'){
 const node=make('button',label,className);node.type='button';node.addEventListener('click',()=>{if(!busy)handler();});return node;
}
function selectButtons(selector,selected){document.querySelectorAll(selector).forEach(b=>{const active=b===selected;b.classList.toggle('selected',active);b.setAttribute('aria-pressed',String(active));});}
let editing=null;
function resetForm(){editing=null;$('student-form').reset();$('form-title').textContent='Add a student';$('save').textContent='Add student +';$('cancel').hidden=true;}
async function load(){
 const data=await api(`/api/students?sort=${$('sort').value}&search=${encodeURIComponent($('search').value.trim())}`);
 $('total').textContent=data.total;$('average').textContent=data.total?`${data.average.toFixed(2)}%`:'—';$('passing').textContent=data.passing;
 $('students').replaceChildren();$('empty').hidden=data.students.length>0;
 if(!data.students.length)$('empty').replaceChildren(make('strong',data.total?'No matching students.':'Every learner starts somewhere.'),make('span',data.total?'Try a different name.':'Add a student and their three subject scores.'));
 for(const student of data.students){const row=make('tr');row.append(make('td',student.name));for(const key of ['mathematics','science','english'])row.append(make('td',student[key]));row.append(make('td',student.average.toFixed(2)));const grade=make('td');grade.append(make('span',student.grade,`badge ${student.grade.toLowerCase()}`));row.append(grade);const actions=make('td');const group=make('div','','row-actions');group.append(button('Edit',()=>{editing=student;for(const id of ['name','mathematics','science','english'])$(id).value=student[id];$('form-title').textContent='Edit student';$('save').textContent='Save changes';$('cancel').hidden=false;$('name').focus();}),button('Delete',()=>{if(confirm(`Delete the record for ${student.name}?`))run(async()=>{await api(`/api/students/${student.id}`,'DELETE');if(editing?.id===student.id)resetForm();await load();message('Student record deleted.');});}));actions.append(group);row.append(actions);$('students').append(row);}
}
$('student-form').addEventListener('submit',event=>{event.preventDefault();run(async()=>{const data={name:$('name').value.trim()};for(const id of ['mathematics','science','english'])data[id]=Number($(id).value);const updating=editing!==null;await api(updating?`/api/students/${editing.id}`:'/api/students',updating?'PUT':'POST',data);resetForm();await load();message(updating?'Scores updated.':'Student added to the class.');});});
$('cancel').addEventListener('click',()=>{if(!busy)resetForm();});
$('search-form').addEventListener('submit',event=>{event.preventDefault();run(async()=>{await load();message('Class view updated.');});});
run(async()=>{await load();message('Your grade book is ready.');});
