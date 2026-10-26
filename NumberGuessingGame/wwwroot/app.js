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
let low=1, high=100, busy=false, won=false;
function lock(value) { busy=value; $('submit').disabled=value||won; $('reset').disabled=value; $('guess').disabled=value||won; }
async function reset() {
 lock(true);
 try { const data=await api('/api/game',{}); low=1; high=100; won=false; $('attempts').textContent='0'; $('range').textContent='1 – 100'; $('orb').textContent='?'; $('headline').textContent='What’s your best guess?'; $('history').replaceChildren(); $('guess').value=''; message(data.message); }
 catch(error){message(error.message,true);}
 finally {lock(false); $('guess').focus();}
}
$('reset').addEventListener('click',reset);
$('guess-form').addEventListener('submit',async event=>{
 event.preventDefault(); if(busy||won)return; const guess=Number($('guess').value);
 if(!Number.isInteger(guess)||guess<1||guess>100){message('Choose a whole number between 1 and 100.',true);return;}
 lock(true);
 try {const data=await api('/api/guess',{guess}); $('attempts').textContent=data.attempts;
 if(data.direction==='correct'){won=true;low=high=guess;$('orb').textContent=guess;$('headline').textContent='Beautifully guessed!';message(`You found ${guess} in ${data.attempts} attempt${data.attempts===1?'':'s'}. Ready for another round?`);}
 else {if(data.direction==='higher')low=Math.max(low,guess+1);else high=Math.min(high,guess-1); message(data.direction==='higher'?'A little higher. You’re getting closer!':'Think lower. Let’s narrow it down.');}
 $('range').textContent=won?String(guess):`${low} – ${high}`; addHistory(`Guess ${data.attempts} · ${guess}`,data.direction==='correct'?'✓ Found it':data.direction==='higher'?'↑ Go higher':'↓ Go lower'); $('guess').value='';
 }catch(error){message(error.message,true);}finally{lock(false);if(!won)$('guess').focus();}
});
reset();
