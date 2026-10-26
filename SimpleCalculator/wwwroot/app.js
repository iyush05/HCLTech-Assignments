const $ = id => document.getElementById(id);
async function api(path, body) {
  let response;
  try { response = await fetch(path, {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(body)}); }
  catch { throw new Error('Unable to connect. Check that the app is running and try again.'); }
  let data; try { data = await response.json(); } catch { throw new Error('The request could not be processed. Check your input and try again.'); }
  if (!response.ok) throw new Error(data.error || 'Something went wrong. Please try again.');
  return data;
}
function message(text, error=false) { $('status').textContent=text; $('status').classList.toggle('error',error); }
function addHistory(left, right) {
  $('empty')?.remove(); const li=document.createElement('li');
  const label=document.createElement('span'), value=document.createElement('strong');
  label.textContent=left; value.textContent=right; li.append(label,value); $('history').prepend(li);
  while ($('history').children.length>15) $('history').lastChild.remove();
}
let operation='+', active=$('first'), busy=false;
const symbols={'+':'+','-':'−','*':'×','/':'÷'};
for(const id of ['first','second'])$(id).addEventListener('focus',()=>active=$(id));
document.querySelectorAll('[data-op]').forEach(button=>button.addEventListener('click',()=>{
 operation=button.dataset.op;document.querySelectorAll('[data-op]').forEach(b=>{b.classList.toggle('selected',b===button);b.setAttribute('aria-pressed',b===button);});
}));
document.querySelectorAll('[data-key]').forEach(button=>button.addEventListener('click',()=>{
 const key=button.dataset.key;
 if(key==='Next'){active=active===$('first')?$('second'):$('first');}
 else if(key==='CE')active.value='';
 else if(key==='⌫')active.value=active.value.slice(0,-1);
 else if(key==='±')active.value=active.value.startsWith('-')?active.value.slice(1):'-'+active.value;
 else if(key!=='.'||!active.value.includes('.'))active.value+=key;
 active.focus();
}));
$('clear').addEventListener('click',()=>{if(busy)return; $('first').value='';$('second').value='';$('result').textContent='0';$('expression').textContent='Ready when you are';$('history').replaceChildren();$('empty')?.remove();message('Workspace cleared. Start something new.');$('first').focus();});
$('calc-form').addEventListener('submit',async event=>{
 event.preventDefault();if(busy)return;busy=true;$('submit').disabled=true;$('clear').disabled=true;
 const first=$('first').value.trim(),second=$('second').value.trim(),op=operation;
 try{const data=await api('/api/calculate',{first,second,operation:op});$('result').textContent=data.result;$('expression').textContent=`${first} ${symbols[op]} ${second}`;addHistory(`${first} ${symbols[op]} ${second}`,`= ${data.result}`);message('Calculation complete.');}
 catch(error){message(error.message,true);$('result').textContent='—';$('expression').textContent='Check your numbers and try again';}
 finally{busy=false;$('submit').disabled=false;$('clear').disabled=false;}
});
