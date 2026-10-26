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
let questions=[],answers={},index=0,finished=false;
function updateNavigation(){const q=questions[index];$('previous').disabled=busy||index===0||finished;$('next').disabled=busy||!q||answers[q.id]===undefined||finished;}
function render(){
 const q=questions[index];if(!q)return;
 $('question-number').textContent=`QUESTION ${index+1} OF ${questions.length}`;$('question-title').textContent=q.prompt;$('answers').replaceChildren();
 q.options.forEach((option,i)=>{const b=button('',()=>{answers[q.id]=i;render();},`answer ${answers[q.id]===i?'selected':''}`);b.setAttribute('aria-pressed',String(answers[q.id]===i));b.append(make('span',String.fromCharCode(65+i),'answer-letter'),make('span',option));$('answers').append(b);});
 $('progress-fill').style.width=`${Object.keys(answers).length/questions.length*100}%`;$('progress').setAttribute('aria-valuenow',Object.keys(answers).length);$('progress').setAttribute('aria-valuemax',questions.length);
 $('next').textContent=index===questions.length-1?'Finish quiz ✓':'Next question →';
 $('question-dots').replaceChildren(...questions.map((q,i)=>{const dot=make('span',i+1,`${answers[q.id]!==undefined?'answered':''} ${i===index?'current':''}`);dot.setAttribute('aria-label',`Question ${i+1}, ${answers[q.id]!==undefined?'answered':'unanswered'}`);return dot;}));updateNavigation();
}
async function loadHistory(){const data=await api('/api/history');$('attempts').textContent=data.total;$('best').textContent=data.total?`${data.best}/${questions.length}`:'—';$('history').replaceChildren(...data.attempts.map(a=>{const li=make('li');li.append(make('span',date(a.date)),make('strong',`${a.score} / ${a.total}`));return li;}));}
$('previous').addEventListener('click',()=>{if(busy||index===0)return;index--;render();});
$('next').addEventListener('click',async()=>{
 if(busy||answers[questions[index].id]===undefined)return;
 if(index<questions.length-1){index++;render();return;}
 await run(async()=>{
  const data=await api('/api/submit','POST',{answers});finished=true;$('quiz').hidden=true;$('results').hidden=false;$('score').textContent=`${data.attempt.score}/${data.attempt.total}`;$('result-title').textContent=data.attempt.score===data.attempt.total?'A perfect little round.':'A little wiser than before.';
  $('review').replaceChildren(...data.review.map(item=>{const section=make('section','','review-item');section.append(make('h3',`${item.isCorrect?'✓':'↗'} ${item.prompt}`),make('p',`Your answer: ${item.selected}`));if(!item.isCorrect)section.append(make('p',`Correct answer: ${item.correct}`));section.append(make('p',item.explanation));return section;}));message('Round complete. Your score has been saved.');await loadHistory();
 });updateNavigation();
});
$('restart').addEventListener('click',()=>{if(busy)return;answers={};index=0;finished=false;$('quiz').hidden=false;$('results').hidden=true;render();message('A new round. A new opportunity.');});
(async()=>{await run(async()=>{questions=await api('/api/questions');render();await loadHistory();message('Take your time. There is no timer.');});updateNavigation();})();
