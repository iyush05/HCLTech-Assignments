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
const categories=[{name:'Distance',units:['Kilometers','Miles'],formulas:['miles = kilometers ÷ 1.609344','kilometers = miles × 1.609344'],refs:[['1 kilometer','≈ 0.621371 miles'],['1 mile','1.609344 kilometers'],['5 kilometers','≈ 3.10686 miles']]},{name:'Weight',units:['Kilograms','Pounds'],formulas:['pounds = kilograms ÷ 0.45359237','kilograms = pounds × 0.45359237'],refs:[['1 kilogram','≈ 2.20462 pounds'],['1 pound','0.45359237 kilograms'],['10 kilograms','≈ 22.0462 pounds']]},{name:'Temperature',units:['Celsius','Fahrenheit'],formulas:['°F = (°C × 1.8) + 32','°C = (°F − 32) ÷ 1.8'],refs:[['Water freezes','0 °C = 32 °F'],['Water boils*','100 °C = 212 °F'],['Absolute zero','−273.15 °C'],['*At standard pressure','']]}];
let category=0,reverse=false,busy=false;
function render(){const c=categories[category];for(const id of ['from','to']){$(id).replaceChildren(...c.units.map((unit,i)=>new Option(unit,String(i))));} $('from').value=reverse?'1':'0';$('to').value=reverse?'0':'1';$('formula').textContent=c.formulas[reverse?1:0];$('reference-category').textContent=c.name;$('reference').replaceChildren(...c.refs.map(([a,b])=>{const div=document.createElement('div');div.className='info-row';const x=document.createElement('span'),y=document.createElement('strong');x.textContent=a;y.textContent=b;div.append(x,y);return div;}));$('result').textContent='—';$('result-unit').textContent='Ready to convert';message('');}
function direction(){reverse=$('from').value==='1';render();}
$('from').addEventListener('change',direction);
$('to').addEventListener('change',()=>{reverse=$('to').value==='0';render();});
$('swap').addEventListener('click',()=>{reverse=!reverse;render();});
document.querySelectorAll('[data-category]').forEach(button=>button.addEventListener('click',()=>{category=Number(button.dataset.category);reverse=false;document.querySelectorAll('[data-category]').forEach(b=>{b.classList.toggle('selected',b===button);b.setAttribute('aria-pressed',b===button);});render();}));
document.querySelectorAll('[data-value]').forEach(button=>button.addEventListener('click',()=>{$('value').value=button.dataset.value;$('result').textContent='—';$('result-unit').textContent='Ready to convert';}));
$('value').addEventListener('input',()=>{$('result').textContent='—';$('result-unit').textContent='Ready to convert';});
$('convert-form').addEventListener('submit',async event=>{
 event.preventDefault();if(busy)return;busy=true;const controls=[...document.querySelectorAll('button,input,select')];controls.forEach(e=>e.disabled=true);
 try{const data=await api('/api/convert',{value:$('value').value.trim(),option:category*2+(reverse?2:1)});$('result').textContent=data.result;$('result-unit').textContent=categories[category].units[reverse?0:1];message('Conversion complete.');}
 catch(error){$('result').textContent='—';$('result-unit').textContent='Check your value';message(error.message,true);}
 finally{busy=false;controls.forEach(e=>e.disabled=false);}
});render();
