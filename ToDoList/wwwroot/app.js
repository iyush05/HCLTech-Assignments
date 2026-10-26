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
let filter='all',editing=null,tasks=[];
function resetForm(){editing=null;$('task-form').reset();$('form-title').textContent='Make a little plan';$('save').textContent='Add task +';$('cancel').hidden=true;}
async function load(){
 const data=await api(`/api/tasks?filter=${filter}&search=${encodeURIComponent($('search').value.trim())}`);tasks=data.tasks;
 for(const key of ['total','active','completed'])$(key).textContent=data[key];
 $('tasks').replaceChildren();$('empty').hidden=tasks.length>0;
 if(!tasks.length)$('empty').replaceChildren(make('strong',data.total?'No matching tasks.':'A little room to think.'),make('span',data.total?'Try another search or filter.':'Add your first task to get started.'));
 for(const task of tasks){
  const row=make('li','',`task-row ${task.completed?'done':''}`),check=make('input');check.type='checkbox';check.checked=task.completed;check.setAttribute('aria-label',`Mark ${task.title} ${task.completed?'incomplete':'complete'}`);
  check.addEventListener('change',()=>{if(busy){check.checked=task.completed;return;}run(async()=>{try{await api(`/api/tasks/${task.id}`,'PUT',{...task,completed:check.checked});if(editing?.id===task.id)editing.completed=check.checked;await load();message('Task updated. A little progress feels good.');}catch(error){check.checked=task.completed;throw error;}});});
  const content=make('div','','task-content'),meta=make('div','','meta');content.append(make('span',task.title,'task-title'));meta.append(make('span',task.priority,`badge ${task.priority.toLowerCase()}`),make('span',date(task.createdAt)));content.append(meta);
  const actions=make('div','','row-actions');actions.append(button('Edit',()=>{editing=task;$('title').value=task.title;$('priority').value=task.priority;$('form-title').textContent='Edit your task';$('save').textContent='Save changes';$('cancel').hidden=false;$('title').focus();}),button('Delete',()=>{if(confirm(`Delete “${task.title}”?`))run(async()=>{await api(`/api/tasks/${task.id}`,'DELETE');if(editing?.id===task.id)resetForm();await load();message('Task deleted.');});}));row.append(check,content,actions);$('tasks').append(row);
 }
}
$('task-form').addEventListener('submit',event=>{event.preventDefault();run(async()=>{const updating=editing!==null;await api(updating?`/api/tasks/${editing.id}`:'/api/tasks',updating?'PUT':'POST',{title:$('title').value.trim(),priority:$('priority').value,completed:editing?.completed??false});resetForm();await load();message(updating?'Task details updated.':'Task added. One step at a time.');});});
$('cancel').addEventListener('click',()=>{if(!busy)resetForm();});
$('search-form').addEventListener('submit',event=>{event.preventDefault();run(async()=>{await load();message('Task list updated.');});});
document.querySelectorAll('[data-filter]').forEach(b=>b.addEventListener('click',()=>{if(busy)return;filter=b.dataset.filter;selectButtons('[data-filter]',b);run(async()=>{await load();message('Task list updated.');});}));
run(async()=>{await load();message('Your list is ready.');});
