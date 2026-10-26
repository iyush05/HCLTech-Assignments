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
let passwords=[];
function updateActions(){$('copy-all').disabled=busy||passwords.length===0;$('clear').disabled=busy||passwords.length===0;}
async function copy(text){try{await navigator.clipboard.writeText(text);message('Copied to your clipboard.');}catch{message('Clipboard access is unavailable. Select the password text and copy it manually.',true);}}
$('length').addEventListener('input',()=>{$('length-value').value=$('length').value;});
$('password-form').addEventListener('submit',async event=>{event.preventDefault();await run(async()=>{
 const options={length:Number($('length').value),count:Number($('count').value)};for(const id of ['uppercase','lowercase','numbers','symbols','excludeAmbiguous'])options[id]=$(id).checked;
 const data=await api('/api/generate','POST',options);passwords=data.passwords;$('empty').hidden=true;
 $('passwords').replaceChildren(...passwords.map((password,i)=>{const item=make('div','','password-item');item.append(make('code',password));const actions=make('div','','password-actions');actions.append(make('span',`PASSWORD ${String(i+1).padStart(2,'0')} · ${password.length} CHARACTERS`),button('Copy',()=>copy(password),''));item.append(actions);return item;}));message(`${passwords.length} fresh password${passwords.length===1?'':'s'} generated. Preferences saved.`);
 });updateActions();});
$('copy-all').addEventListener('click',()=>{if(!busy)copy(passwords.join('\n'));});
$('clear').addEventListener('click',()=>{if(busy)return;passwords=[];$('passwords').replaceChildren();$('empty').hidden=false;updateActions();message('Results cleared from the page.');});
(async()=>{await run(async()=>{const settings=await api('/api/preferences');$('length').value=settings.length;$('length-value').value=settings.length;$('count').value=settings.count;for(const id of ['uppercase','lowercase','numbers','symbols','excludeAmbiguous'])$(id).checked=settings[id];message('Your preferences are ready.');});updateActions();})();
