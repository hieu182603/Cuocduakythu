// ==========================================
// GAMEPLAY MAP, TURNS & EVENT MODALS LOGIC
// ==========================================

const MAP_ICONS = {
q:`<svg viewBox="0 0 100 100">
  <defs>
    <radialGradient id="questionFace" cx="34%" cy="24%" r="78%">
      <stop offset="0" stop-color="#a75be0"/><stop offset=".58" stop-color="#6d27a8"/><stop offset="1" stop-color="#45116f"/>
    </radialGradient>
    <linearGradient id="questionRim" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#5a2289"/><stop offset="1" stop-color="#2b0a4e"/>
    </linearGradient>
  </defs>
  <ellipse cx="50" cy="84" rx="28" ry="7" fill="#3a1650" opacity=".28"/>
  <circle cx="50" cy="48" r="39" fill="url(#questionRim)" stroke="#311052" stroke-width="3"/>
  <circle cx="50" cy="47" r="33" fill="url(#questionFace)" stroke="#c99bea" stroke-width="2.4"/>
  <ellipse cx="39" cy="28" rx="12" ry="6" fill="#fff" opacity=".28" transform="rotate(-18 39 28)"/>
  <text x="50" y="66" text-anchor="middle" font-family="'Baloo 2','Arial Rounded MT Bold',sans-serif" font-size="51"
        font-weight="800" fill="#fff7dc" stroke="#3b0b63" stroke-width="2.2" paint-order="stroke">?</text>
</svg>`,

reward:`<svg viewBox="0 0 100 100">
  <defs>
    <linearGradient id="giftBody" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#ffe13d"/><stop offset="1" stop-color="#f4a900"/>
    </linearGradient>
    <linearGradient id="giftLid" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#fff05a"/><stop offset="1" stop-color="#ffbc0d"/>
    </linearGradient>
    <linearGradient id="giftRibbon" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="#73c51f"/><stop offset="1" stop-color="#2c8c18"/>
    </linearGradient>
  </defs>
  <ellipse cx="50" cy="84" rx="33" ry="7" fill="#31520f" opacity=".3"/>
  <rect x="21" y="47" width="58" height="35" rx="4" fill="url(#giftBody)" stroke="#80540a" stroke-width="3"/>
  <path d="M24 51H76V59H24Z" fill="#d98600" opacity=".32"/>
  <rect x="43" y="47" width="14" height="35" fill="url(#giftRibbon)" stroke="#206e14" stroke-width="2.2"/>
  <rect x="16" y="35" width="68" height="17" rx="5" fill="url(#giftLid)" stroke="#80540a" stroke-width="3"/>
  <rect x="42" y="35" width="16" height="17" fill="url(#giftRibbon)" stroke="#206e14" stroke-width="2.2"/>
  <path d="M49 34C40 34 27 31 28 23C29 17 38 18 43 22C47 25 49 30 49 34Z" fill="#ffcc18" stroke="#80540a" stroke-width="2.6"/>
  <path d="M51 34C60 34 73 31 72 23C71 17 62 18 57 22C53 25 51 30 51 34Z" fill="#ffcc18" stroke="#80540a" stroke-width="2.6"/>
  <circle cx="50" cy="32" r="6.5" fill="#4fa918" stroke="#206e14" stroke-width="2.4"/>
  <path d="M27 56V72" stroke="#fff8a8" stroke-width="3" stroke-linecap="round" opacity=".55"/>
</svg>`,

trap:`<svg viewBox="0 0 100 100">
  <defs>
    <linearGradient id="trapSteel" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#dbe2e8"/><stop offset=".5" stop-color="#7c8792"/><stop offset="1" stop-color="#3d454d"/>
    </linearGradient>
  </defs>
  <ellipse cx="50" cy="84" rx="37" ry="7" fill="#38100b" opacity=".34"/>
  <path d="M17 61Q50 30 83 61L78 69Q50 50 22 69Z" fill="url(#trapSteel)" stroke="#242a30" stroke-width="3.2"/>
  <path d="M24 62L30 43L38 61L43 38L51 59L58 38L64 61L71 43L77 62" fill="#eef2f4" stroke="#242a30" stroke-width="2.2" stroke-linejoin="round"/>
  <ellipse cx="50" cy="70" rx="27" ry="10" fill="#7d2119" stroke="#242a30" stroke-width="3"/>
  <path d="M17 69Q50 94 83 69L78 82Q50 96 22 82Z" fill="url(#trapSteel)" stroke="#242a30" stroke-width="3.2"/>
  <path d="M25 78L31 59L39 79L45 56L51 78L58 56L64 79L71 59L77 78" fill="#eef2f4" stroke="#242a30" stroke-width="2.2" stroke-linejoin="round"/>
  <circle cx="18" cy="69" r="6" fill="#59636d" stroke="#242a30" stroke-width="3"/>
  <circle cx="82" cy="69" r="6" fill="#59636d" stroke="#242a30" stroke-width="3"/>
  <path d="M22 55Q50 34 78 55" fill="none" stroke="#fff" stroke-width="2.4" opacity=".4"/>
</svg>`,

bomb:`<svg viewBox="0 0 100 100">
  <defs>
    <radialGradient id="bombBody" cx="34%" cy="26%" r="78%">
      <stop offset="0" stop-color="#59616a"/><stop offset=".34" stop-color="#252a30"/><stop offset="1" stop-color="#090b0e"/>
    </radialGradient>
  </defs>
  <ellipse cx="44" cy="86" rx="28" ry="7" fill="#3e100b" opacity=".32"/>
  <circle cx="44" cy="59" r="29" fill="url(#bombBody)" stroke="#17191d" stroke-width="3.5"/>
  <ellipse cx="33" cy="46" rx="9" ry="6" fill="#fff" opacity=".38" transform="rotate(-28 33 46)"/>
  <rect x="51" y="25" width="17" height="14" rx="3" fill="#68727b" stroke="#252a30" stroke-width="2.8" transform="rotate(28 59.5 32)"/>
  <path d="M66 27Q74 13 84 19Q90 23 85 29" fill="none" stroke="#3e250f" stroke-width="7" stroke-linecap="round"/>
  <path d="M66 27Q74 13 84 19Q90 23 85 29" fill="none" stroke="#c27622" stroke-width="3.5" stroke-linecap="round"/>
  <polygon points="86,27 90,18 93,24 100,24 95,30 98,37 90,34 84,40 84,32 77,29" fill="#ffe02b" stroke="#f04a17" stroke-width="2" stroke-linejoin="round"/>
  <circle cx="89" cy="29" r="3.6" fill="#ff7a18"/>
</svg>`,

wheel: (()=>{
  const cols=['#ffd22d','#f05b6f','#3aa8e8','#7746c3','#ffb321','#ed4e74','#389bd7','#7042b7'];
  const pt=a=>[50+32*Math.cos(a*Math.PI/180), 50+32*Math.sin(a*Math.PI/180)];
  let w='';
  for(let i=0;i<8;i++){
    const [x1,y1]=pt(i*45-90),[x2,y2]=pt((i+1)*45-90);
    w+=`<path d="M50,50 L${x1.toFixed(1)},${y1.toFixed(1)} A32 32 0 0 1 ${x2.toFixed(1)},${y2.toFixed(1)} Z" fill="${cols[i]}" stroke="#fff4c9" stroke-width="1.5"/>`;
  }
  return `<svg viewBox="0 0 100 100">
    <ellipse cx="50" cy="88" rx="31" ry="6" fill="#123d68" opacity=".28"/>
    <circle cx="50" cy="50" r="43" fill="#40206a" stroke="#251044" stroke-width="3"/>
    <circle cx="50" cy="50" r="37" fill="#f5de9e" stroke="#fff2c5" stroke-width="2"/>
    <g class="wheelSpin">${w}<circle cx="50" cy="50" r="8" fill="#4a2474" stroke="#fff2c5" stroke-width="2.6"/><circle cx="50" cy="50" r="3" fill="#ffd22d"/></g>
    <circle cx="50" cy="50" r="32" fill="none" stroke="#3b1a62" stroke-width="2"/>
    <polygon points="50,4 42,18 58,18" fill="#ffd22d" stroke="#3b1a62" stroke-width="2.6" stroke-linejoin="round"/>
  </svg>`;
})(),

start:`<svg viewBox="0 0 190 100" aria-hidden="true">
  <defs>
    <pattern id="startChecks" width="16" height="16" patternUnits="userSpaceOnUse">
      <rect width="16" height="16" fill="#fff9e8"/>
      <rect width="8" height="8" fill="#20252a"/>
      <rect x="8" y="8" width="8" height="8" fill="#20252a"/>
    </pattern>
    <linearGradient id="startGold" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="#ffe45b"/><stop offset="1" stop-color="#c99510"/>
    </linearGradient>
  </defs>
  <path d="M42 90Q18 69 31 40" fill="none" stroke="#9a7010" stroke-width="3" stroke-linecap="round"/>
  <path d="M148 90Q172 69 159 40" fill="none" stroke="#9a7010" stroke-width="3" stroke-linecap="round"/>
  <g fill="url(#startGold)" stroke="#8b650d" stroke-width="1.2">
    <ellipse cx="36" cy="80" rx="9" ry="3.8" transform="rotate(42 36 80)"/>
    <ellipse cx="27" cy="72" rx="9" ry="3.8" transform="rotate(25 27 72)"/>
    <ellipse cx="25" cy="61" rx="9" ry="3.8" transform="rotate(-4 25 61)"/>
    <ellipse cx="29" cy="50" rx="9" ry="3.8" transform="rotate(-30 29 50)"/>
    <ellipse cx="154" cy="80" rx="9" ry="3.8" transform="rotate(-42 154 80)"/>
    <ellipse cx="163" cy="72" rx="9" ry="3.8" transform="rotate(-25 163 72)"/>
    <ellipse cx="165" cy="61" rx="9" ry="3.8" transform="rotate(4 165 61)"/>
    <ellipse cx="161" cy="50" rx="9" ry="3.8" transform="rotate(30 161 50)"/>
  </g>
  <path d="M68 68L78 9" stroke="#f4e6b9" stroke-width="6" stroke-linecap="round"/>
  <path d="M68 68L78 9" stroke="#49351e" stroke-width="3" stroke-linecap="round"/>
  <circle cx="78" cy="8" r="4" fill="#ffd84a" stroke="#49351e" stroke-width="2"/>
  <path d="M78 13C98 6 116 15 140 8L136 41C116 47 99 36 72 44Z" fill="#183a18" opacity=".28" transform="translate(2 3)"/>
  <path d="M78 13C98 6 116 15 140 8L136 41C116 47 99 36 72 44Z" fill="url(#startChecks)" stroke="#20252a" stroke-width="2.6" stroke-linejoin="round"/>
  <path d="M82 15Q102 10 122 14" fill="none" stroke="#fff" stroke-width="2" opacity=".38"/>
  <g stroke="#7a580d" stroke-width="1">
    <g transform="translate(48 88)">
      <circle r="3" fill="#ffd43d"/><circle cx="-4" cy="0" r="3.2" fill="#ff9fc2"/><circle cx="4" cy="0" r="3.2" fill="#fff5eb"/><circle cx="0" cy="-4" r="3.2" fill="#fff5eb"/>
    </g>
    <g transform="translate(142 88)">
      <circle r="3" fill="#ffd43d"/><circle cx="-4" cy="0" r="3.2" fill="#fff5eb"/><circle cx="4" cy="0" r="3.2" fill="#ff9fc2"/><circle cx="0" cy="-4" r="3.2" fill="#fff5eb"/>
    </g>
  </g>
</svg>`,
};

function buildSceneHTML() {
  const W=1100,H=640,cx=550,cy=412,rx=312,ry=136;
  const N=26;
  let posts='';
  for(let i=0;i<N;i++){
    const a=i/N*2*Math.PI;
    const px=cx+(rx-14)*Math.cos(a), py=cy+(ry-8)*Math.sin(a);
    const sc=.85+.35*(Math.sin(a)+1)/2;
    posts+=`<g transform="translate(${px.toFixed(1)},${py.toFixed(1)}) scale(${sc.toFixed(2)})">
      <rect x="-3.5" y="-21" width="7" height="24" rx="3" fill="#fbfbfb" stroke="#b9b4ae" stroke-width="1.6"/></g>`;
  }
  const tree=(x,y,s)=>`<g transform="translate(${x},${y}) scale(${s})">
    <rect x="-6" y="8" width="12" height="26" rx="4" fill="#8a5a2e" stroke="#6d451f" stroke-width="2"/>
    <circle cx="-16" cy="0" r="20" fill="#4ea836"/><circle cx="16" cy="2" r="18" fill="#4ea836"/>
    <circle cx="0" cy="-14" r="23" fill="#63bf45"/><circle cx="-6" cy="-18" r="9" fill="#7ed45e" opacity=".8"/></g>`;
  const flowers=(x,y)=>{
    const cols=['#f26bb5','#ffd23f','#fff','#f0402e','#b678ee'];
    let f='';for(let i=0;i<5;i++)f+=`<circle cx="${x+i*13-26}" cy="${y+(i%2?5:0)}" r="4" fill="${cols[i]}" stroke="#00000022"/>`;
    return f;
  };
  const balloon=(x,y,s,c1,c2,cls)=>`<g class="balloon ${cls}" ><g transform="translate(${x},${y}) scale(${s})">
    <ellipse cx="0" cy="0" rx="26" ry="30" fill="${c1}" stroke="#00000030" stroke-width="2"/>
    <path d="M0,-30 C 11,-30 15,30 0,30 C -15,30 -11,-30 0,-30" fill="${c2}"/>
    <path d="M-26,0 Q0,12 26,0" fill="none" stroke="#00000025" stroke-width="2"/>
    <line x1="-12" y1="26" x2="-8" y2="42" stroke="#7a5a30" stroke-width="2"/>
    <line x1="12" y1="26" x2="8" y2="42" stroke="#7a5a30" stroke-width="2"/>
    <rect x="-10" y="42" width="20" height="13" rx="3" fill="#a06a2c" stroke="#7a4c1a" stroke-width="2"/></g></g>`;
  const cloud=(x,y,s,cls)=>`<g class="cloud ${cls}"><g transform="translate(${x},${y}) scale(${s})" fill="#fff" opacity=".95">
    <ellipse cx="0" cy="0" rx="34" ry="15"/><ellipse cx="-20" cy="4" rx="20" ry="11"/>
    <ellipse cx="20" cy="5" rx="22" ry="11"/><ellipse cx="2" cy="-9" rx="20" ry="12"/></g></g>`;
  const castle=`<g transform="translate(915,182)">
    <ellipse cx="0" cy="150" rx="120" ry="30" fill="#8cc94f"/>
    <path d="M-40,150 Q-90,190 -130,225" fill="none" stroke="#e8cf9a" stroke-width="16" stroke-linecap="round" opacity=".95"/>
    <rect x="-55" y="55" width="26" height="95" fill="#f3ead8" stroke="#c9b892" stroke-width="2.5"/>
    <polygon points="-58,57 -26,57 -42,18" fill="#3a86e0" stroke="#2a62a8" stroke-width="2"/>
    <rect x="29" y="55" width="26" height="95" fill="#f3ead8" stroke="#c9b892" stroke-width="2.5"/>
    <polygon points="26,57 58,57 42,18" fill="#e84040" stroke="#b02525" stroke-width="2"/>
    <rect x="-30" y="40" width="60" height="110" fill="#faf3e3" stroke="#c9b892" stroke-width="2.5"/>
    <polygon points="-34,42 34,42 0,-10" fill="#e84040" stroke="#b02525" stroke-width="2"/>
    <line x1="0" y1="-10" x2="0" y2="-30" stroke="#8a5a2e" stroke-width="3"/>
    <polygon points="0,-30 22,-24 0,-17" fill="#ffd23f" stroke="#c98a0a" stroke-width="1.5"/>
    <path d="M-12,150 L-12,112 A12 12 0 0 1 12,112 L12,150 Z" fill="#7a5230" stroke="#5a3b1e" stroke-width="2"/>
    <circle cx="0" cy="70" r="7" fill="#7ec8f0" stroke="#4a7fa8" stroke-width="2"/>
    <circle cx="-42" cy="80" r="5" fill="#7ec8f0" stroke="#4a7fa8" stroke-width="1.6"/>
    <circle cx="42" cy="80" r="5" fill="#7ec8f0" stroke="#4a7fa8" stroke-width="1.6"/>
  </g>`;
  let cabins='';
  const cabCols=['#f2c530','#e84a4a','#3a86e0','#58b847','#9b59d0','#f27ab0','#f28c2e','#38c2c9'];
  for(let i=0;i<8;i++){
    const a=i/8*2*Math.PI;
    cabins+=`<circle cx="${(58*Math.cos(a)).toFixed(1)}" cy="${(58*Math.sin(a)).toFixed(1)}" r="9" fill="${cabCols[i]}" stroke="#00000035" stroke-width="2"/>`;
  }
  let spokes='';
  for(let i=0;i<8;i++){
    const a=i/8*2*Math.PI;
    spokes+=`<line x1="0" y1="0" x2="${(58*Math.cos(a)).toFixed(1)}" y2="${(58*Math.sin(a)).toFixed(1)}" stroke="#e8703a" stroke-width="3.5"/>`;
  }
  const ferris=`<g transform="translate(718,235)">
    <polygon points="0,0 -30,95 -18,95 0,14 18,95 30,95" fill="#c85a20"/>
    <g class="fw">
      <circle r="58" fill="none" stroke="#e8703a" stroke-width="5"/>
      <circle r="44" fill="none" stroke="#f0925e" stroke-width="2.5"/>
      ${spokes}${cabins}
      <circle r="8" fill="#b5541e" stroke="#8a3c10" stroke-width="2"/>
    </g></g>`;
  const tent=`<g transform="translate(600,318)">
    <rect x="-26" y="0" width="52" height="24" fill="#fff" stroke="#c94040" stroke-width="2"/>
    <rect x="-18" y="0" width="10" height="24" fill="#e84a4a"/><rect x="8" y="0" width="10" height="24" fill="#e84a4a"/>
    <polygon points="-34,0 34,0 0,-42" fill="#e84a4a" stroke="#b02525" stroke-width="2"/>
    <polygon points="-16,0 0,-42 0,0" fill="#fff" opacity=".85"/>
    <line x1="0" y1="-42" x2="0" y2="-54" stroke="#8a5a2e" stroke-width="2.5"/>
    <polygon points="0,-54 14,-50 0,-45" fill="#ffd23f"/></g>`;
  const stand=`<g transform="translate(88,212)">
    <rect x="0" y="42" width="238" height="62" rx="6" fill="#c08b4a" stroke="#8a5c24" stroke-width="3"/>
    <rect x="8" y="50" width="222" height="34" rx="4" fill="#9a6a30"/>
    <text x="119" y="82" text-anchor="middle" font-size="34">🦛🦁🐻🐰🐘</text>
    <rect x="0" y="96" width="238" height="16" rx="5" fill="#d8a45c" stroke="#8a5c24" stroke-width="2.5"/>
    <polygon points="-12,44 250,44 236,2 2,2" fill="#f28c2e" stroke="#c05a10" stroke-width="3"/>
    <polygon points="26,44 250,44 236,2 60,2" fill="#fff" opacity=".28"/>
    <path d="M-12,44 h262" stroke="#c05a10" stroke-width="3"/>
    ${[0,1,2,3,4,5,6].map(i=>`<polygon points="${-4+i*38},46 ${14+i*38},46 ${5+i*38},58" fill="${['#e84a4a','#ffd23f','#3a86e0','#58b847'][i%4]}" stroke="#00000025"/>`).join('')}
  </g>`;
  const mushroom=`<g transform="translate(150,398)">
    <rect x="-26" y="-18" width="52" height="52" rx="16" fill="#f7ecd2" stroke="#cbb28a" stroke-width="2.5"/>
    <path d="M-58,-14 A58 34 0 0 1 58,-14 Z" fill="#e84343" stroke="#b02525" stroke-width="3"/>
    <circle cx="-28" cy="-26" r="8" fill="#fff" opacity=".92"/>
    <circle cx="6" cy="-36" r="10" fill="#fff" opacity=".92"/>
    <circle cx="36" cy="-24" r="7" fill="#fff" opacity=".92"/>
    <path d="M-10,34 L-10,10 A10 10 0 0 1 10,10 L10,34 Z" fill="#7a5230" stroke="#5a3b1e" stroke-width="2"/>
    <circle cx="0" cy="18" r="2" fill="#e8b83a"/></g>`;
  const bridge=`<g transform="translate(768,568)">
    <path d="M-66,36 L-66,10 Q0,-34 66,10 L66,36 Z" fill="#cfc4b2" stroke="#8f8272" stroke-width="3.5"/>
    <path d="M-38,36 Q0,-8 38,36 Z" fill="#2f7fb8"/>
    <path d="M-66,10 Q0,-34 66,10" fill="none" stroke="#8f8272" stroke-width="3"/>
    <line x1="-40" y1="4" x2="-40" y2="16" stroke="#8f8272" stroke-width="2"/>
    <line x1="0" y1="-10" x2="0" y2="2" stroke="#8f8272" stroke-width="2"/>
    <line x1="40" y1="4" x2="40" y2="16" stroke="#8f8272" stroke-width="2"/></g>`;
  const bench=`<g transform="translate(1022,470)">
    <rect x="-28" y="-26" width="56" height="8" rx="3" fill="#a06a2c" stroke="#7a4c1a" stroke-width="2"/>
    <rect x="-28" y="-12" width="56" height="9" rx="3" fill="#b87c38" stroke="#7a4c1a" stroke-width="2"/>
    <rect x="-24" y="-4" width="7" height="16" fill="#7a4c1a"/><rect x="17" y="-4" width="7" height="16" fill="#7a4c1a"/></g>`;
  const lamp=`<g transform="translate(972,398)">
    <rect x="-3" y="0" width="6" height="66" fill="#3a4652"/>
    <circle cx="0" cy="-8" r="11" fill="#ffe89a" stroke="#3a4652" stroke-width="3"/>
    <circle cx="0" cy="-8" r="17" fill="#ffe89a" opacity=".25"/>
    <rect x="-8" y="-24" width="16" height="6" rx="2" fill="#3a4652"/></g>`;
  const sign=`<g transform="translate(148,520)">
    <rect x="-4" y="-30" width="8" height="52" rx="3" fill="#8a5a2e" stroke="#6d451f" stroke-width="2"/>
    <polygon points="-30,-30 22,-30 34,-21 22,-12 -30,-12" fill="#d8a45c" stroke="#8a5c24" stroke-width="2.5"/>
    <line x1="-22" y1="-21" x2="14" y2="-21" stroke="#8a5c24" stroke-width="3" stroke-linecap="round"/></g>`;
  let rails=`<ellipse cx="${cx}" cy="${cy-15}" rx="${rx-14}" ry="${ry-8}" fill="none" stroke="#f4f4f4" stroke-width="4" opacity=".95"/>
             <ellipse cx="${cx}" cy="${cy-6}"  rx="${rx-14}" ry="${ry-8}" fill="none" stroke="#efeef0" stroke-width="4" opacity=".95"/>`;

  return `<svg class="bg" viewBox="0 0 ${W} ${H}" preserveAspectRatio="xMidYMid slice" style="position:absolute;inset:0;width:100%;height:100%;z-index:0;">
  <defs>
    <linearGradient id="sky" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#5fc2f5"/><stop offset=".65" stop-color="#a8ddfa"/><stop offset="1" stop-color="#c8ecfc"/>
    </linearGradient>
    <linearGradient id="grass" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#8ed05c"/><stop offset="1" stop-color="#57a532"/>
    </linearGradient>
    <linearGradient id="dirt" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#e3b269"/><stop offset="1" stop-color="#c8924a"/>
    </linearGradient>
    <linearGradient id="water" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#5ec1ee"/><stop offset="1" stop-color="#2f8fc9"/>
    </linearGradient>
  </defs>
  <rect width="${W}" height="${H}" fill="url(#sky)"/>
  ${cloud(180,66,1,'')}${cloud(520,44,.8,'c2')}${cloud(900,72,1.05,'c3')}${cloud(710,110,.55,'c2')}
  <polygon points="60,20 64,29 73,32 64,35 60,44 56,35 47,32 56,29" fill="#fff" opacity=".8"/>
  <polygon points="1000,140 1003,147 1010,149 1003,152 1000,159 997,152 990,149 997,147" fill="#ffe89a" opacity=".9"/>
  <polygon points="420,120 423,127 430,129 423,132 420,139 417,132 410,129 417,127" fill="#fff" opacity=".7"/>
  <ellipse cx="220" cy="392" rx="380" ry="120" fill="#a5d867"/>
  <ellipse cx="850" cy="400" rx="420" ry="130" fill="#b4e07a"/>
  <rect x="0" y="352" width="${W}" height="${H-352}" fill="url(#grass)"/>
  ${balloon(140,108,.9,'#e84a4a','#ffd23f','')}
  ${balloon(330,70,.65,'#3a86e0','#fff','b2')}
  ${balloon(1005,105,.75,'#f28c2e','#fff','b2')}
  ${castle}
  ${ferris}
  ${tent}
  ${stand}
  ${mushroom}
  ${tree(320,300,.85)}${tree(56,468,.95)}${tree(1052,330,.9)}${tree(492,318,.6)}
  <path d="M0,580 C150,548 300,612 460,584 C620,556 700,626 860,596 C960,578 1040,600 1100,586 L1100,640 L0,640 Z"
        fill="url(#water)" stroke="#2477a8" stroke-width="4"/>
  <path class="ripple" d="M120,600 q22,-8 44,0 M340,614 q22,-8 44,0 M560,606 q22,-8 44,0 M920,612 q22,-8 44,0"
        fill="none" stroke="#dff4ff" stroke-width="3" stroke-linecap="round"/>
  <text x="252" y="606" font-size="26">🦆</text>
  <text x="392" y="600" font-size="30">🦩</text>
  <text x="540" y="602" font-size="30">⛵</text>
  ${bridge}
  <text x="905" y="588" font-size="52">🐘</text>
  <text x="878" y="548" font-size="20">💦</text>
  ${bench}${lamp}${sign}
  <g>${flowers(230,556)}${flowers(96,552)}${flowers(1058,548)}</g>
  <ellipse cx="${cx}" cy="${cy+12}" rx="${rx+10}" ry="${ry+8}" fill="#00000018"/>
  <ellipse cx="${cx}" cy="${cy}" rx="${rx}" ry="${ry}" fill="url(#dirt)" stroke="#a8763a" stroke-width="7"/>
  <ellipse cx="${cx}" cy="${cy}" rx="${rx-42}" ry="${ry-30}" fill="#d9a75f" opacity=".7"/>
  <ellipse cx="470" cy="380" rx="60" ry="16" fill="#b8823c" opacity=".25"/>
  <ellipse cx="660" cy="460" rx="70" ry="18" fill="#b8823c" opacity=".2"/>
  ${rails}${posts}
</svg>`;
}

const TYPE_LABELS = {
    q: 'CÂU HỎI',
    reward: 'THƯỞNG',
    trap: 'BẪY',
    bomb: 'BẪY',
    wheel: 'VÒNG QUAY',
    start: 'START / FINISH'
};

function renderBoard() {
    const board = document.getElementById("board");
    if (!board) return;
    board.innerHTML = "";

    // 1. Vẽ 40 ô cơ bản + 1 ô start
    for (let n = 1; n <= 40; n++) {
        const tile = TILE_COORDS[n];
        const el = document.createElement('div');
        el.className = `tile ${tile.type}`;
        el.id = `tile-${n}`;
        el.style.gridArea = `${tile.row} / ${tile.col}`;
        
        const iconContent = tile.type === 'bomb' ? MAP_ICONS.bomb : (MAP_ICONS[tile.type] || '');
        el.innerHTML = `
            <div class="num">${String(n).padStart(2, '0')}</div>
            <div class="icon">${iconContent}</div>
            <div class="label">${TYPE_LABELS[tile.type]}</div>
        `;
        board.appendChild(el);
    }

    // Ô START/FINISH rộng 2 cột (dòng 8, cột 1 và 2)
    const startEl = document.createElement('div');
    startEl.className = 'tile start';
    startEl.id = 'tile-0';
    startEl.style.gridArea = '8 / 1 / 9 / 3';
    startEl.innerHTML = `
        <div class="icon">${MAP_ICONS.start}</div>
        <div class="startTxt"><b>START</b><span>FINISH</span></div>
    `;
    board.appendChild(startEl);

    // 2. Tạo cảnh trung tâm #scene
    const scene = document.createElement('div');
    scene.id = 'scene';
    scene.innerHTML = `
        <img class="scene-artwork" src="assets/map/nen.png" alt="Khung cảnh trung tâm Cuộc Đua Kỳ Thú" decoding="async" fetchpriority="high">
        <div id="sceneTimer">
            <div class="time-box total-time" title="Tổng thời gian trận đấu">
                <span class="t-icon">⏱️</span>
                <span id="lblTotalTime">10:00</span>
            </div>
        </div>
        <div id="diceArea">
            <div id="center-interactive-info" class="center-interactive-info">Khởi tạo lượt chơi...</div>
            <div class="dice-row">
                <div class="dice-box">
                    <div class="dice-cube" id="visual-dice-1">
                        <div class="dice-face face-1"><div class="pip"></div></div>
                        <div class="dice-face face-2"><div class="pip"></div><div class="pip"></div></div>
                        <div class="dice-face face-3"><div class="pip"></div><div class="pip"></div><div class="pip"></div></div>
                        <div class="dice-face face-4"><div class="pip"></div><div class="pip"></div><div class="pip"></div><div class="pip"></div></div>
                        <div class="dice-face face-5"><div class="pip"></div><div class="pip"></div><div class="pip"></div><div class="pip"></div><div class="pip"></div></div>
                        <div class="dice-face face-6"><div class="pip"></div><div class="pip"></div><div class="pip"></div><div class="pip"></div><div class="pip"></div><div class="pip"></div></div>
                    </div>
                </div>
            </div>
            <button id="btn-roll-dice">🎲 Tung Xúc Xắc</button>
            <div id="dice-modifiers-desc" style="color:#fff; font-size:0.95cqi; text-shadow:0 1px 2px rgba(0,0,0,0.5); min-height:1.2cqi;"></div>
        </div>
    `;
    board.appendChild(scene);

    // 3. Gán sự kiện cho các nút điều khiển
    safeAddListener("btn-roll-dice", "click", handleRollDice);
    safeAddListener("btnExit", "click", () => {
        showConfirm("Bạn có chắc muốn thoát ván chơi này? Tiến trình chơi sẽ bị mất.", () => {
            if (isOnlineMode && connection) {
                localStorage.removeItem("saved_room_code");
                window.location.reload();
            } else {
                showScreen("menu");
            }
        });
    });

    safeAddListener("btnHelp", "click", () => {
        const modal = document.getElementById("rulesModal");
        if (modal) modal.classList.add("show");
    });

    safeAddListener("rulesClose", "click", () => {
        const modal = document.getElementById("rulesModal");
        if (modal) modal.classList.remove("show");
    });

    safeAddListener("rulesCloseX", "click", () => {
        const modal = document.getElementById("rulesModal");
        if (modal) modal.classList.remove("show");
    });

    const soundBtn = document.getElementById("btnSound");
    if (soundBtn) {
        updateGameplaySoundButton();
        soundBtn.onclick = () => {
            isGameplayMuted = !isGameplayMuted;
            localStorage.setItem("map_muted", isGameplayMuted);
            updateGameplaySoundButton();
        };
    }

    // Tự động sắp xếp lại quân cờ khi thay đổi kích thước màn hình
    window.removeEventListener('resize', updatePlayerPositionsOnBoard);
    window.addEventListener('resize', updatePlayerPositionsOnBoard);
}

const OFFS = [[-1,-1],[1,-1],[-1,1],[1,1]];

function placeToken(p, tokenEl, racersList) {
    const board = document.getElementById("board");
    const tile = document.getElementById(`tile-${p.tileIndex}`);
    if (!tile || !tokenEl || !board) return;

    const boardRect = board.getBoundingClientRect();
    const tileRect = tile.getBoundingClientRect();
    const occupants = racersList.filter(other => other.tileIndex === p.tileIndex);
    const slot = occupants.indexOf(p);

    if (p.tileIndex === 0) {
        if (!p.startAnchor) {
            const seedX = Math.abs(Math.sin(p.id + 1));
            const seedY = Math.abs(Math.sin(p.id + 2));
            p.startAnchor = {
                x: 0.12 + seedX * 0.76,
                y: 0.23 + seedY * 0.38,
                layer: p.id
            };
        }
        
        const count = occupants.length;
        const scale = count > 30 ? 0.26 : count > 15 ? 0.32 : count > 8 ? 0.38 : 0.48;
        const size = Math.min(tileRect.height * scale, tileRect.width * (scale * 0.52));
        
        tokenEl.style.setProperty('--token-size', size + 'px');
        tokenEl.style.setProperty('--badge-size', Math.max(10, size * 0.42) + 'px');
        tokenEl.style.setProperty('--badge-font', Math.max(7, size * 0.24) + 'px');
        tokenEl.style.setProperty('--badge-border', Math.max(1, size * 0.04) + 'px');
        
        const tokenHeight = tokenEl.offsetHeight || size / 0.75;
        const centerX = Math.max(size * 0.55, Math.min(tileRect.width - size * 0.55, tileRect.width * p.startAnchor.x));
        const rawTop = tileRect.height * p.startAnchor.y - tokenHeight / 2;
        const top = Math.max(2, Math.min(tileRect.height - tokenHeight - 2, rawTop));
        
        tokenEl.style.left = (tileRect.left - boardRect.left + centerX - size / 2) + 'px';
        tokenEl.style.top = (tileRect.top - boardRect.top + top) + 'px';
        tokenEl.style.zIndex = String(32 + Math.round(p.startAnchor.y * 20 + p.startAnchor.layer % 4));
        return;
    }

    tokenEl.style.removeProperty('--token-size');
    tokenEl.style.removeProperty('--badge-size');
    tokenEl.style.removeProperty('--badge-font');
    tokenEl.style.removeProperty('--badge-border');
    
    const offset = OFFS[slot % OFFS.length];
    const layer = Math.floor(slot / OFFS.length);
    const spread = tileRect.width * 0.17;
    const nudge = Math.min(layer, 5) * tileRect.width * 0.018;
    
    tokenEl.style.left = (tileRect.left - boardRect.left + tileRect.width / 2 + offset[0] * spread + nudge - tokenEl.offsetWidth / 2) + 'px';
    tokenEl.style.top = (tileRect.top - boardRect.top + tileRect.height / 2 + offset[1] * spread + nudge - tokenEl.offsetHeight / 2) + 'px';
    tokenEl.style.zIndex = String(30 + Math.min(layer, 20));
}

function updatePlayerPositionsOnBoard() {
    const board = document.getElementById("board");
    if (!board) return;

    const racers = players.filter(p => !p.isSpectator);

    racers.forEach((p, i) => {
        let token = document.getElementById(`player-token-${p.id}`);
        
        if (!token) {
            token = document.createElement('div');
            token.className = 'token';
            token.id = `player-token-${p.id}`;
            
            const imgPath = p.character.image || `assets/characters/locphat.png`;
            token.innerHTML = `<img src="${imgPath}" alt="${p.name}"><b style="background:${p.character.color}">${i+1}</b>`;
            board.appendChild(token);
        }

        placeToken(p, token, racers);
    });

    const allTokens = board.querySelectorAll(".token");
    allTokens.forEach(t => {
        const pIdStr = t.id.replace("player-token-", "");
        const pId = parseInt(pIdStr);
        if (!isNaN(pId)) {
            const stillExists = racers.some(p => p.id === pId);
            if (!stillExists) {
                board.removeChild(t);
            }
        }
    });
}

let audioCtx = null;

function initAudioContext() {
    if (audioCtx) return;
    audioCtx = new (window.AudioContext || window.webkitAudioContext)();
}

function playSFX(type) {
    if (isGameplayMuted) return;
    initAudioContext();
    try {
        if (audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
        let osc = audioCtx.createOscillator();
        let gain = audioCtx.createGain();
        osc.connect(gain);
        gain.connect(audioCtx.destination);
        
        if (type === 'click') {
            osc.type = 'sine';
            osc.frequency.setValueAtTime(500, audioCtx.currentTime);
            osc.frequency.exponentialRampToValueAtTime(800, audioCtx.currentTime + 0.08);
            gain.gain.setValueAtTime(0.03, audioCtx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, audioCtx.currentTime + 0.08);
            osc.start();
            osc.stop(audioCtx.currentTime + 0.08);
        } else if (type === 'success') {
            osc.type = 'sine';
            osc.frequency.setValueAtTime(523.25, audioCtx.currentTime);
            osc.frequency.setValueAtTime(659.25, audioCtx.currentTime + 0.08);
            osc.frequency.setValueAtTime(783.99, audioCtx.currentTime + 0.16);
            gain.gain.setValueAtTime(0.04, audioCtx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, audioCtx.currentTime + 0.3);
            osc.start();
            osc.stop(audioCtx.currentTime + 0.3);
        } else if (type === 'fail') {
            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(180, audioCtx.currentTime);
            osc.frequency.linearRampToValueAtTime(90, audioCtx.currentTime + 0.35);
            gain.gain.setValueAtTime(0.05, audioCtx.currentTime);
            gain.gain.exponentialRampToValueAtTime(0.001, audioCtx.currentTime + 0.35);
            osc.start();
            osc.stop(audioCtx.currentTime + 0.35);
        }
    } catch(e) {}
}

let gameplayBgmAudio = null;
let isGameplayMuted = localStorage.getItem("map_muted") === "true";

function initGameplayAudio() {
    if (!gameplayBgmAudio) {
        gameplayBgmAudio = new Audio("sound/tatamusic-gaming.mp3");
        gameplayBgmAudio.loop = true;
        gameplayBgmAudio.volume = 0.45;
    }
}

function playGameplayBGM() {
    initGameplayAudio();
    if (isGameplayMuted) return;
    gameplayBgmAudio.play().catch(e => console.log("BGM Play prevented:", e.message));
}

function stopGameplayBGM() {
    if (gameplayBgmAudio) {
        gameplayBgmAudio.pause();
    }
}

function updateGameplaySoundButton() {
    const btn = document.getElementById('btnSound');
    if (!btn) return;
    if (isGameplayMuted) {
        btn.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="width:52%;height:52%;overflow:visible">
          <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"></polygon>
          <line x1="23" y1="9" x2="17" y2="15"></line>
          <line x1="17" y1="9" x2="23" y2="15"></line>
        </svg>`;
        btn.classList.add('muted');
        stopGameplayBGM();
    } else {
        btn.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" style="width:52%;height:52%;overflow:visible">
          <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"></polygon>
          <path d="M19.07 4.93a10 10 0 0 1 0 14.14M15.54 8.46a5 5 0 0 1 0 7.07"></path>
        </svg>`;
        btn.classList.remove('muted');
        playGameplayBGM();
    }
}

document.addEventListener('click', () => {
    initGameplayAudio();
    if (gameplayBgmAudio && gameplayBgmAudio.paused && !isGameplayMuted) {
        playGameplayBGM();
    }
}, { once: true });

function renderScoreboard() {
    const list = document.getElementById("scoreboard-list");
    if (!list) return;
    list.innerHTML = "";

    // Sort players by position (lapCount descending, then tileIndex descending, filter out spectators)
    const sorted = [...players]
        .filter(p => !p.isSpectator)
        .sort((a, b) => {
            if (b.lapCount !== a.lapCount) {
                return b.lapCount - a.lapCount;
            }
            return b.tileIndex - a.tileIndex;
        });

    sorted.forEach((p, index) => {
        const row = document.createElement("div");
        row.className = "scoreboard-item";
        
        let statusBadges = "";
        if(p.shield) statusBadges += `<i class="fa-solid fa-shield-halved sb-shield" title="Có Lá Chắn"></i>`;
        if(p.skipTurn) statusBadges += `<i class="fa-solid fa-ban sb-skip" title="Mất Lượt"></i>`;
        if(p.doubleDice) statusBadges += `<i class="fa-solid fa-angles-up sb-shield" style="color:var(--warning);" title="Nhân đôi xúc xắc"></i>`;

        row.innerHTML = `
            <div class="sb-player-info">
                <div class="sb-avatar" style="background-color: ${p.character.color}">${p.character.icon}</div>
                <div class="sb-details">
                    <span class="sb-name">${p.name} ${statusBadges}</span>
                    <span class="sb-char">${p.character.name}</span>
                </div>
            </div>
            <div class="sb-stats">
                Vòng <span class="sb-lap" style="color:var(--success); font-weight:900;">${p.lapCount || 0}</span> / Ô <span class="sb-tile">${p.tileIndex + 1}</span>
            </div>
        `;
        list.appendChild(row);
    });
}

// Setup Turn State
function setupTurn(playerIndex) {
    const player = players[playerIndex];
    if (!player) return;
    
    // Close all event modals to keep spectators and active players in sync
    const questionModal = document.getElementById("question-modal");
    if (questionModal) questionModal.classList.remove("active");
    
    const trapModal = document.getElementById("trap-modal");
    if (trapModal) trapModal.classList.remove("active");
    
    const rewardModal = document.getElementById("reward-modal");
    if (rewardModal) rewardModal.classList.remove("active");
    
    const wheelModal = document.getElementById("wheel-modal");
    if (wheelModal) wheelModal.classList.remove("active");
    
    stopQuestionTimer();
    
    // Update Scoreboard active highlight
    renderScoreboard();

    // Highlight active landing tile
    document.querySelectorAll(".tile").forEach(t => t.classList.remove("active-landing"));
    const activeTile = document.getElementById(`tile-${player.tileIndex}`);
    if (activeTile) activeTile.classList.add("active-landing");

    const currentPlayerName = document.getElementById("current-player-name");
    if (currentPlayerName) {
        currentPlayerName.innerText = player.name;
        currentPlayerName.style.color = player.character.color;
    }
    
    const currentPlayerCharDesc = document.getElementById("current-player-char-desc");
    if (currentPlayerCharDesc) {
        currentPlayerCharDesc.innerText = `${player.character.name} (${player.character.badge})`;
    }
    
    // Render status effects
    const effectsContainer = document.getElementById("current-player-effects");
    if (effectsContainer) {
        effectsContainer.innerHTML = "";
        if (player.shield) effectsContainer.innerHTML += `<span class="effect-badge shield">Lá chắn</span>`;
        if (player.skipTurn) effectsContainer.innerHTML += `<span class="effect-badge skip">Bị Choáng/Mất lượt</span>`;
        if (player.doubleDice) effectsContainer.innerHTML += `<span class="effect-badge double">Double Dice</span>`;
        if (player.diceModifier > 0) effectsContainer.innerHTML += `<span class="effect-badge skip">Xúc xắc tối đa 3 (${player.diceModifier} lượt)</span>`;
    }

    const isActivePlayer = isOnlineMode ? (player.id === myPlayerId) : true;

    const btnRollDice = document.getElementById("btn-roll-dice");
    const centerInfo = document.getElementById("center-interactive-info");

    // Handle skip turn state
    if (player.skipTurn) {
        logMessage(`Tay đua [${player.name}] bị mất lượt trong vòng này.`, "log-trap");
        player.skipTurn = false; // Reset skip state
        if (btnRollDice) btnRollDice.disabled = true;
        if (centerInfo) centerInfo.innerText = `Mất lượt! Lượt chuyển sang người kế tiếp.`;
        
        if(!isOnlineMode) {
            setTimeout(() => {
                nextTurn();
            }, 2000);
        }
    } else {
        if(isActivePlayer) {
            if (btnRollDice) btnRollDice.disabled = false;
            if (centerInfo) centerInfo.innerText = `Đến lượt bạn! Hãy tung xúc xắc để di chuyển [${player.name}]!`;
        } else {
            if (btnRollDice) btnRollDice.disabled = true;
            if (centerInfo) centerInfo.innerText = `Đang đợi lượt di chuyển của [${player.name}]...`;
        }
    }

    // Reset visual dice values
    const visualDice1 = document.getElementById("visual-dice-1");
    if (visualDice1) {
        visualDice1.style.transform = "rotateX(0deg) rotateY(0deg) rotateZ(0deg)";
        visualDice1.classList.remove("dice-shake");
    }
    
    const diceModifiersDesc = document.getElementById("dice-modifiers-desc");
    if (diceModifiersDesc) diceModifiersDesc.innerText = "";
}

function nextTurn() {
    activePlayerIndex = (activePlayerIndex + 1) % players.length;
    setupTurn(activePlayerIndex);
}

function roll3DDice(visualDice, value) {
    if (!visualDice) return;
    const rotations = {
        1: { x: 0, y: 0 },       // Front: face-1 (1 dot)
        2: { x: 0, y: 180 },     // Front: face-2 (2 dots)
        3: { x: 0, y: 90 },      // Front: face-3 (3 dots)
        4: { x: 0, y: -90 },     // Front: face-4 (4 dots)
        5: { x: -90, y: 0 },     // Front: face-5 (5 dots)
        6: { x: 90, y: 0 }       // Front: face-6 (6 dots)
    };
    const rot = rotations[value] || rotations[1];
    const extraX = (Math.floor(Math.random() * 3) + 3) * 360;
    const extraY = (Math.floor(Math.random() * 3) + 3) * 360;
    const extraZ = (Math.floor(Math.random() * 3) + 3) * 360;
    
    const finalX = rot.x + extraX;
    const finalY = rot.y + extraY;
    visualDice.style.transform = `rotateX(${finalX}deg) rotateY(${finalY}deg) rotateZ(${extraZ}deg)`;
}

// Rolling Dice physics & state triggers
function handleRollDice() {
    document.getElementById("btn-roll-dice").disabled = true;
    if (isOnlineMode) {
        connection.invoke("RollDice", roomCode)
            .catch(err => console.error("Error rolling dice: ", err));
        return;
    }
    const player = players[activePlayerIndex];
    document.getElementById("btn-roll-dice").disabled = true;

    const dice1 = document.getElementById("visual-dice-1");
    
    // Roll calculations
    let rollVal1 = Math.floor(Math.random() * 6) + 1;
    
    if (player.diceModifier > 0) {
        rollVal1 = Math.min(3, rollVal1);
        player.diceModifier--;
    }

    // Trigger 3D roll animations
    roll3DDice(dice1, rollVal1);
    document.getElementById("dice-modifiers-desc").innerText = `Đang tung xúc xắc...`;

    let totalMove = rollVal1;
    let hadDouble = player.doubleDice;
    
    // Double Dice reward
    if(player.doubleDice) {
        totalMove = rollVal1 * 2;
        player.doubleDice = false;
    }

    setTimeout(() => {
        if(hadDouble) {
            document.getElementById("dice-modifiers-desc").innerText = `Double Dice: ${rollVal1} x 2 = ${totalMove} ô`;
        } else {
            document.getElementById("dice-modifiers-desc").innerText = `Di chuyển: ${totalMove} ô`;
        }

        logMessage(`[${player.name}] xúc xắc được ${rollVal1} (Di chuyển: ${totalMove} ô).`);

        // Move Player token
        movePlayerSequentially(player, totalMove);

    }, 1500); // Wait 1.5s for 3D roll transition to complete
}

// Smooth sequential hop movement
function movePlayerSequentially(player, steps) {
    let currentStep = 0;
    
    let stepDelay = 400; // milliseconds
    if(speedSetting === "fast") stepDelay = 200;
    if(speedSetting === "instant") stepDelay = 0;

    function doStep() {
        if (currentStep < steps) {
            // Increment tile index
            player.tileIndex = (player.tileIndex + 1) % TILE_COORDS.length;
            
            // Check if player passed start tile (Start is index 0)
            if (player.tileIndex === 0) {
                player.lapCompleted = true;
            }

            // Highlight active landing tile
            document.querySelectorAll(".tile").forEach(t => t.classList.remove("active-landing"));
            const activeTile = document.getElementById(`tile-${player.tileIndex}`);
            if (activeTile) activeTile.classList.add("active-landing");

            // Animate local hop
            updatePlayerPositionsOnBoard();
            const token = document.getElementById(`player-token-${player.id}`);
            if (token && speedSetting !== "instant") {
                token.classList.add("horse-hop");
                setTimeout(() => token.classList.remove("horse-hop"), 300);
            }

            currentStep++;
            setTimeout(doStep, stepDelay);
        } else {
            // Movement completed, activate tile effect
            activateTile(player);
        }
    }

    doStep();
}

function activateTile(player) {
    const tile = TILE_COORDS[player.tileIndex];
    document.getElementById("center-interactive-info").innerText = `[${player.name}] đã dừng ở ô số ${player.tileIndex + 1} (${tile.name})`;

    // Check if player landed on START/FINISH and completed a lap to WIN
    if(player.tileIndex === 0 && player.lapCompleted) {
        triggerVictory(player);
        return;
    }

    switch(tile.type) {
        case "start":
            // Landing exactly on start (without passing it)
            logMessage(`[${player.name}] đã hạ cánh an toàn ở vạch START/FINISH.`, "log-win");
            setTimeout(nextTurn, 1500);
            break;
        case "question":
            triggerQuestionEvent(player);
            break;
        case "trap":
            triggerTrapEvent(player);
            break;
        case "reward":
            triggerRewardEvent(player);
            break;
        case "wheel":
            triggerWheelEvent(player);
            break;
    }
}

// Event A: Question popup
let currentQuestion = null;
let questionTimerInterval = null;
let questionTimeRemaining = 30;

function startQuestionTimer(duration, onTimeout) {
    stopQuestionTimer();
    questionTimeRemaining = duration;
    
    const timerText = document.getElementById("question-timer-text");
    const timerBar = document.getElementById("question-timer-bar");
    
    if (timerText) timerText.innerText = `${questionTimeRemaining}s`;
    if (timerBar) {
        timerBar.style.width = "100%";
    }
    
    const startTime = Date.now();
    const endTime = startTime + duration * 1000;
    
    questionTimerInterval = setInterval(() => {
        const now = Date.now();
        const timeLeft = Math.max(0, endTime - now);
        const secondsLeft = Math.ceil(timeLeft / 1000);
        
        if (timerText) timerText.innerText = `${secondsLeft}s`;
        if (timerBar) {
            const percentage = (timeLeft / (duration * 1000)) * 100;
            timerBar.style.width = `${percentage}%`;
        }
        
        if (timeLeft <= 0) {
            stopQuestionTimer();
            if (onTimeout) onTimeout();
        }
    }, 100);
}

function stopQuestionTimer() {
    if (questionTimerInterval) {
        clearInterval(questionTimerInterval);
        questionTimerInterval = null;
    }
}

function triggerQuestionEvent(player) {
    if (questions.length === 0) {
        logMessage("Không có câu hỏi nào trong kho. Bỏ qua lượt hỏi.", "log-question");
        setTimeout(nextTurn, 1500);
        return;
    }

    // Select random question
    const qIndex = Math.floor(Math.random() * questions.length);
    currentQuestion = questions[qIndex];

    const modal = document.getElementById("question-modal");
    document.getElementById("question-text").innerText = currentQuestion.question;
    
    // Streak warning
    const streakWarn = document.getElementById("wrong-streak-warning");
    if(player.wrongStreak > 0) {
        streakWarn.innerText = `⚠️ CHUỖI SAI: ${player.wrongStreak} lần! (Phạt cộng thêm ${player.wrongStreak * 5} giây chờ)`;
        streakWarn.style.display = "block";
    } else {
        streakWarn.style.display = "none";
    }

    // Populate answers
    const answersGrid = document.getElementById("answers-grid");
    answersGrid.innerHTML = "";

    currentQuestion.answers.forEach((ans, idx) => {
        const btn = document.createElement("button");
        btn.className = "answer-btn";
        btn.innerHTML = `${String.fromCharCode(65 + idx)}. ${ans}`;
        btn.addEventListener("click", () => handleAnswerSelected(btn, idx));
        answersGrid.appendChild(btn);
    });

    modal.classList.add("active");

    // Khởi chạy đếm ngược 30 giây (Offline)
    startQuestionTimer(30, () => {
        logMessage(`[${player.name}] đã HẾT GIỜ trả lời câu hỏi!`, "log-trap");
        
        // Disable all buttons
        const allButtons = document.querySelectorAll(".answer-btn");
        allButtons.forEach(btn => btn.disabled = true);
        
        // Show correct button
        allButtons[currentQuestion.correct].classList.add("correct");
        
        player.wrongStreak++;
        
        // Apply random penalty
        let penalties = ["lùi", "mất lượt", "chờ"];
        let penaltyType = penalties[Math.floor(Math.random() * penalties.length)];
        let penaltyDesc = "";
        
        if (penaltyType === "lùi") {
            let backSteps = Math.floor(Math.random() * 3) + 1; // 1-3 tiles
            player.tileIndex = Math.max(0, player.tileIndex - backSteps);
            penaltyDesc = `lùi ${backSteps} ô`;
        } else if(penaltyType === "mất lượt") {
            player.skipTurn = true;
            penaltyDesc = `mất lượt ở vòng tiếp theo`;
        } else {
            let seconds = 10 + (player.wrongStreak * 5);
            penaltyDesc = `chờ thêm ${seconds} giây hình phạt`;
        }

        logMessage(`Hình phạt hết giờ: ${penaltyDesc}.`, "log-trap");

        setTimeout(() => {
            document.getElementById("question-modal").classList.remove("active");
            updatePlayerPositionsOnBoard();
            nextTurn();
        }, 4000);
    });
}

function handleAnswerSelected(selectedBtn, answerIndex) {
    stopQuestionTimer(); // Dừng đếm ngược ngay lập tức
    const player = players[activePlayerIndex];
    const allButtons = document.querySelectorAll(".answer-btn");
    
    // Disable all buttons to prevent double click
    allButtons.forEach(btn => btn.disabled = true);

    const isCorrect = answerIndex === currentQuestion.correct;
    
    if (isCorrect) {
        selectedBtn.classList.add("correct");
        playSFX('success');
        player.wrongStreak = 0; // reset streak
        logMessage(`[${player.name}] đã trả lời ĐÚNG câu hỏi trắc nghiệm!`, "log-question");
        
        setTimeout(() => {
            document.getElementById("question-modal").classList.remove("active");
            nextTurn();
        }, 4000);
    } else {
        selectedBtn.classList.add("incorrect");
        playSFX('fail');
        
        // Show correct button
        allButtons[currentQuestion.correct].classList.add("correct");
        
        player.wrongStreak++;
        
        // Apply random penalty
        let penalties = ["lùi", "mất lượt", "chờ"];
        let penaltyType = penalties[Math.floor(Math.random() * penalties.length)];
        
        let penaltyDesc = "";
        
        if (penaltyType === "lùi") {
            let backSteps = Math.floor(Math.random() * 3) + 1; // 1-3 tiles
            player.tileIndex = Math.max(0, player.tileIndex - backSteps);
            penaltyDesc = `lùi ${backSteps} ô`;
        } else if(penaltyType === "mất lượt") {
            player.skipTurn = true;
            penaltyDesc = `mất lượt ở vòng tiếp theo`;
        } else {
            // delay seconds
            let seconds = 10 + (player.wrongStreak * 5);
            penaltyDesc = `chờ thêm ${seconds} giây hình phạt`;
        }

        logMessage(`[${player.name}] trả lời SAI! Chuỗi sai liên tiếp: ${player.wrongStreak}. Hình phạt: ${penaltyDesc}.`, "log-trap");

        setTimeout(() => {
            document.getElementById("question-modal").classList.remove("active");
            updatePlayerPositionsOnBoard();
            nextTurn();
        }, 4000);
    }
}

// Event B: TRAP events
const TRAP_LIST = [
    { type: "lùi", name: "Hố Bùn Lầy", detail: "Nhựa bị trượt chân! Lùi ngay 2 ô.", value: 2 },
    { type: "lùi-lớn", name: "Động Đất Trượt Dốc", detail: "Sự cố địa chấn hung bạo! Lùi ngay 5 ô.", value: 5 },
    { type: "mất-lượt", name: "Đầm Lầy Choáng Váng", detail: "Nhân vật bị mắc kẹt bùn sâu, mất lượt ở vòng sau.", value: 1 },
    { type: "giảm-xúc-xắc", name: "Bảo Táp Gió Ngược", detail: "Gió bão cản trở! Xúc xắc lăn tối đa chỉ được 3 trong 2 lượt tới.", value: 2 },
    { type: "đổi-vị-trí", name: "Cổng Dịch Chuyển Lỗi", detail: "Cổng không gian lỗi! Tráo đổi vị trí của bạn với một người chơi ngẫu nhiên.", value: 0 },
    { type: "quay-start", name: "Hố Đen Vũ Trụ (Hiếm)", detail: "Trôi dạt không gian! Bị hút trực tiếp quay về vạch Start.", value: 99 }
];

function triggerTrapEvent(player) {
    // Check for shield block
    if(player.shield) {
        player.shield = false;
        logMessage(`Lá Chắn của [${player.name}] đã kích hoạt thành công, giúp chặn đứng bẫy hiểm họa!`, "log-reward");
        
        // Setup shield block modal
        document.getElementById("reward-modal-desc").innerText = "Lá chắn kích hoạt!";
        document.getElementById("reward-cards-container").style.display = "none";
        document.getElementById("reward-result-box").style.display = "block";
        document.getElementById("reward-result-box").innerHTML = `
            <div class="effect-name" style="color:var(--success);">KÍCH HOẠT LÁ CHẮN!</div>
            <div class="effect-detail">Bẫy hiểm họa đã bị hóa giải hoàn toàn bằng khiên phòng ngự!</div>
        `;
        const closeBtn = document.getElementById("btn-close-reward-modal");
        closeBtn.style.display = "inline-flex";
        closeBtn.onclick = () => {
            document.getElementById("reward-modal").classList.remove("active");
            nextTurn();
        };
        document.getElementById("reward-modal").classList.add("active");
        return;
    }

    // Select random trap
    let trapIdx = Math.floor(Math.random() * TRAP_LIST.length);
    if (TRAP_LIST[trapIdx].type === "quay-start" && Math.random() > 0.15) {
        trapIdx = 0; // swap to simple lùi 2
    }
    const trap = TRAP_LIST[trapIdx];

    // Hide result box and close button, reset desc text, show cards container
    document.getElementById("trap-modal-desc").innerText = "Ôi không! Bạn đã dẫm phải bẫy hiểm họa. Hãy chọn 1 lá bài!";
    document.getElementById("trap-result-box").style.display = "none";
    document.getElementById("btn-close-trap-modal").style.display = "none";
    const cardsContainer = document.getElementById("trap-cards-container");
    cardsContainer.style.display = "flex";
    cardsContainer.innerHTML = "";

    // Generate 3 cards data. One of them will be the actual trap, other 2 are random alternatives
    let alternatives = TRAP_LIST.filter(t => t.name !== trap.name);
    alternatives.sort(() => Math.random() - 0.5);
    let cardChoices = [trap, alternatives[0], alternatives[1]];
    cardChoices.sort(() => Math.random() - 0.5);

    let hasSelected = false;

    // Render cards
    cardChoices.forEach((choice, index) => {
        const card = document.createElement("div");
        card.className = "event-card";
        card.innerHTML = `
            <div class="card-back">
                <i class="fa-solid fa-skull-crossbones"></i>
                <span>Bẫy ${index + 1}</span>
            </div>
            <div class="card-front">
                <div class="card-front-icon"><i class="fa-solid fa-triangle-exclamation"></i></div>
                <div class="effect-name">${choice.name}</div>
                <div class="effect-detail">${choice.detail}</div>
            </div>
        `;

        card.addEventListener("click", () => {
            if (hasSelected) return;
            hasSelected = true;
            playSFX('fail');

            const cardInnerElements = cardsContainer.querySelectorAll(".event-card");
            cardInnerElements.forEach((c, idx) => {
                c.classList.add("disabled");
                const frontName = c.querySelector(".effect-name");
                const frontDetail = c.querySelector(".effect-detail");
                
                if (idx === index) {
                    frontName.innerText = trap.name;
                    frontDetail.innerText = trap.detail;
                    c.classList.add("flipped");
                } else {
                    let alternativeIndex = idx < index ? idx : idx - 1;
                    frontName.innerText = alternatives[alternativeIndex].name;
                    frontDetail.innerText = alternatives[alternativeIndex].detail;
                    c.classList.add("flipped");
                    c.classList.add("faded");
                }
            });

            applyTrapEffect(player, trap);

            setTimeout(() => {
                document.getElementById("trap-modal-desc").innerText = "Chi tiết bẫy kích hoạt:";
                document.getElementById("trap-result-box").style.display = "block";
                document.getElementById("trap-result-box").innerHTML = `
                    <div class="effect-name">${trap.name}</div>
                    <div class="effect-detail">${trap.detail}</div>
                `;
                document.getElementById("btn-close-trap-modal").style.display = "inline-flex";
                document.getElementById("btn-close-trap-modal").onclick = () => {
                    document.getElementById("trap-modal").classList.remove("active");
                    const tile = TILE_COORDS[player.tileIndex];
                    if (tile.type !== "start") {
                        activateTile(player);
                    } else {
                        nextTurn();
                    }
                };
            }, 800);
        });

        cardsContainer.appendChild(card);
    });

    document.getElementById("trap-modal").classList.add("active");
}

function applyTrapEffect(player, trap) {
    if (trap.type === "lùi") {
        player.tileIndex = Math.max(0, player.tileIndex - trap.value);
    } else if (trap.type === "lùi-lớn") {
        player.tileIndex = Math.max(0, player.tileIndex - trap.value);
    } else if (trap.type === "mất-lượt") {
        player.skipTurn = true;
    } else if (trap.type === "giảm-xúc-xắc") {
        player.diceModifier = trap.value;
    } else if (trap.type === "quay-start") {
        player.tileIndex = 0;
    } else if (trap.type === "đổi-vị-trí") {
        let candidates = players.filter(p => p.id !== player.id);
        if(candidates.length > 0) {
            let target = candidates[Math.floor(Math.random() * candidates.length)];
            let temp = player.tileIndex;
            player.tileIndex = target.tileIndex;
            target.tileIndex = temp;
            logMessage(`[${player.name}] đã tráo đổi vị trí với [${target.name}].`, "log-trap");
        }
    }

    updatePlayerPositionsOnBoard();
    renderScoreboard();
    logMessage(`[${player.name}] kích hoạt bẫy: ${trap.name} - ${trap.detail}`, "log-trap");
}

// Event C: REWARD events
const REWARD_LIST = [
    { type: "tiến", name: "Cà Rốt Siêu Cấp", detail: "Tăng tốc lực! Tiến lên thêm 3 ô.", value: 3 },
    { type: "lá-chắn", name: "Khiên Thần Bảo Hộ", detail: "Nhận một lớp lá chắn miễn nhiễm hoàn toàn hình phạt từ Bẫy tiếp theo.", value: 1 },
    { type: "thêm-lượt", name: "Động Cơ Phản Lực", detail: "Quá phấn khích! Đi thêm một lượt xúc xắc ngay lập tức.", value: 1 },
    { type: "double-dice", name: "Nhân Đôi Động Cơ", detail: "Double Dice! Lượt tiếp theo xúc xắc của bạn sẽ được nhân đôi khoảng cách.", value: 1 },
    { type: "troll", name: "Quà Troll Bí Mật", detail: "Mở quà... Ồ không! Bạn bị troll, lùi lại 2 ô.", value: -2 },
    { type: "troll-nothing", name: "Hộp Quà Trống", detail: "Mở hộp quà... Không có gì cả! Chúc bạn may mắn lần sau.", value: 0 }
];

function triggerRewardEvent(player) {
    let rIdx = Math.floor(Math.random() * REWARD_LIST.length);
    const reward = REWARD_LIST[rIdx];

    // Hide result box and close button, reset desc text, show cards container
    document.getElementById("reward-modal-desc").innerText = "Tuyệt vời! Bạn nhận được một phần quà may mắn. Hãy chọn 1 lá bài!";
    document.getElementById("reward-result-box").style.display = "none";
    document.getElementById("btn-close-reward-modal").style.display = "none";
    const cardsContainer = document.getElementById("reward-cards-container");
    cardsContainer.style.display = "flex";
    cardsContainer.innerHTML = "";

    // Generate 3 cards data. One is actual reward, other 2 are alternatives
    let alternatives = REWARD_LIST.filter(r => r.name !== reward.name);
    alternatives.sort(() => Math.random() - 0.5);
    let cardChoices = [reward, alternatives[0], alternatives[1]];
    cardChoices.sort(() => Math.random() - 0.5);

    let hasSelected = false;

    // Render cards
    cardChoices.forEach((choice, index) => {
        const card = document.createElement("div");
        card.className = "event-card";
        card.innerHTML = `
            <div class="card-back">
                <i class="fa-solid fa-gift"></i>
                <span>Thưởng ${index + 1}</span>
            </div>
            <div class="card-front">
                <div class="card-front-icon"><i class="fa-solid fa-circle-check"></i></div>
                <div class="effect-name">${choice.name}</div>
                <div class="effect-detail">${choice.detail}</div>
            </div>
        `;

        card.addEventListener("click", () => {
            if (hasSelected) return;
            hasSelected = true;
            playSFX('success');

            const cardInnerElements = cardsContainer.querySelectorAll(".event-card");
            cardInnerElements.forEach((c, idx) => {
                c.classList.add("disabled");
                const frontName = c.querySelector(".effect-name");
                const frontDetail = c.querySelector(".effect-detail");
                
                if (idx === index) {
                    frontName.innerText = reward.name;
                    frontDetail.innerText = reward.detail;
                    c.classList.add("flipped");
                } else {
                    let alternativeIndex = idx < index ? idx : idx - 1;
                    frontName.innerText = alternatives[alternativeIndex].name;
                    frontDetail.innerText = alternatives[alternativeIndex].detail;
                    c.classList.add("flipped");
                    c.classList.add("faded");
                }
            });

            applyRewardEffect(player, reward);

            setTimeout(() => {
                document.getElementById("reward-modal-desc").innerText = "Chi tiết phần thưởng:";
                document.getElementById("reward-result-box").style.display = "block";
                document.getElementById("reward-result-box").innerHTML = `
                    <div class="effect-name">${reward.name}</div>
                    <div class="effect-detail">${reward.detail}</div>
                `;
                document.getElementById("btn-close-reward-modal").style.display = "inline-flex";
                document.getElementById("btn-close-reward-modal").onclick = () => {
                    document.getElementById("reward-modal").classList.remove("active");
                    const tile = TILE_COORDS[player.tileIndex];
                    if (tile.type !== "start") {
                        activateTile(player);
                    } else {
                        nextTurn();
                    }
                };
            }, 800);
        });

        cardsContainer.appendChild(card);
    });

    document.getElementById("reward-modal").classList.add("active");
}

function applyRewardEffect(player, reward) {
    if(reward.type === "tiến") {
        let oldIndex = player.tileIndex;
        player.tileIndex = (player.tileIndex + reward.value) % TILE_COORDS.length;
        if (player.tileIndex < oldIndex) {
            player.lapCompleted = true;
            logMessage(`[${player.name}] đã vượt qua vạch xuất phát nhờ phần thưởng!`, "log-win");
        }
    } else if(reward.type === "lá-chắn") {
        player.shield = true;
    } else if(reward.type === "thêm-lượt") {
        player.isExtraTurn = true;
    } else if(reward.type === "double-dice") {
        player.doubleDice = true;
    } else if(reward.type === "troll") {
        player.tileIndex = Math.max(0, player.tileIndex + reward.value);
    }

    updatePlayerPositionsOnBoard();
    renderScoreboard();
    logMessage(`[${player.name}] kích hoạt thưởng: ${reward.name} - ${reward.detail}`, "log-reward");

    // Adjust turn index if extra turn won
    if(reward.type === "thêm-lượt") {
        activePlayerIndex = (activePlayerIndex - 1 + players.length) % players.length;
    }
}

// Event D: SPIN WHEEL
const WHEEL_SECTOR_COLORS = [
    "#ef4444", "#10b981", "#3b82f6", "#f59e0b", 
    "#ef4444", "#10b981", "#3b82f6", "#f59e0b",
    "#ef4444", "#10b981", "#3b82f6", "#f59e0b"
];

const WHEEL_SECTORS = [
    { label: "Lùi 3 ô", type: "trap", value: -3, desc: "Lùi ngay 3 ô trên bảng." },
    { label: "Tiến 3 ô", type: "reward", value: 3, desc: "Tiến lên 3 ô trên bảng." },
    { label: "Mất lượt", type: "trap", value: "skip", desc: "Mất lượt chơi tiếp theo." },
    { label: "Thêm lượt", type: "reward", value: "extra", desc: "Được xoay xúc xắc thêm lượt nữa!" },
    { label: "Lùi 2 ô", type: "trap", value: -2, desc: "Lùi lại 2 ô." },
    { label: "Nhận Khiên", type: "reward", value: "shield", desc: "Nhận 1 lá chắn bảo hộ." },
    { label: "Xúc xắc x2", type: "reward", value: "double", desc: "Lượt tới khoảng cách xúc xắc nhân đôi." },
    { label: "Quay về Start", type: "trap", value: "start", desc: "Ôi xui xẻo! Quay về vạch xuất phát." },
    { label: "Tiến 2 ô", type: "reward", value: 2, desc: "Tiến lên 2 ô." },
    { label: "Mất Khiên", type: "trap", value: "lose-shield", desc: "Bị tịch thu lá chắn đang có (nếu có)." },
    { label: "Tiến 5 ô", type: "reward", value: 5, desc: "Bứt tốc cực mạnh! Tiến lên 5 ô." },
    { label: "Lùi 5 ô", type: "trap", value: -5, desc: "Trượt dài ngã ngựa! Lùi lại 5 ô." }
];

let isWheelSpinning = false;

function triggerWheelEvent(player) {
    const modal = document.getElementById("wheel-modal");
    if (!modal) return;
    document.getElementById("wheel-result-text").style.display = "none";
    document.getElementById("btn-spin-wheel").style.display = "inline-flex";
    document.getElementById("btn-spin-wheel").disabled = false;
    document.getElementById("btn-close-wheel-modal").style.display = "none";
    
    drawWheel(0);
    modal.classList.add("active");
    
    // Attach listener
    document.getElementById("btn-spin-wheel").onclick = () => {
        spinWheel(player);
    };
}

function drawWheel(angle) {
    const canvas = document.getElementById("wheel-canvas");
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    const width = canvas.width;
    const height = canvas.height;
    const cx = width / 2;
    const cy = height / 2;
    const rOuter = cx - 5;    // Outer border radius
    const rWheel = rOuter - 15; // Actual wheel radius
    
    ctx.clearRect(0, 0, width, height);
    
    // 1. Draw Outer Carnival Rim (dark rim with bulbs)
    ctx.beginPath();
    ctx.arc(cx, cy, rOuter, 0, Math.PI * 2);
    ctx.fillStyle = "#1e293b";
    ctx.fill();
    ctx.lineWidth = 4;
    ctx.strokeStyle = "#3c2f2f";
    ctx.stroke();
    
    // Draw 12 light bulbs on the outer rim
    for (let i = 0; i < 12; i++) {
        const bulbAngle = i * (Math.PI * 2 / 12);
        const bx = cx + (rOuter - 8) * Math.cos(bulbAngle);
        const by = cy + (rOuter - 8) * Math.sin(bulbAngle);
        
        ctx.beginPath();
        ctx.arc(bx, by, 4, 0, Math.PI * 2);
        ctx.fillStyle = i % 2 === 0 ? "#ffd200" : "#ffffff";
        ctx.shadowColor = "#ffd200";
        ctx.shadowBlur = 6;
        ctx.fill();
    }
    ctx.shadowBlur = 0; // Reset shadow
    
    // 2. Draw Wheel Segments
    const numSectors = WHEEL_SECTORS.length;
    const arc = Math.PI * 2 / numSectors;
    
    ctx.save();
    ctx.translate(cx, cy);
    ctx.rotate(angle);
    
    for (let i = 0; i < numSectors; i++) {
        const sectorAngle = i * arc;
        const sector = WHEEL_SECTORS[i];
        
        ctx.beginPath();
        ctx.moveTo(0, 0);
        ctx.arc(0, 0, rWheel, sectorAngle, sectorAngle + arc);
        ctx.closePath();
        
        // Semantic coloring based on type and utility
        if (sector.type === "reward") {
            if (sector.value === "extra" || sector.value === "double") {
                ctx.fillStyle = "#3b82f6"; // Royal Blue
            } else if (sector.value === "shield") {
                ctx.fillStyle = "#f59e0b"; // Amber Gold
            } else {
                ctx.fillStyle = "#10b981"; // Emerald Green
            }
        } else {
            if (sector.value === "start") {
                ctx.fillStyle = "#dc2626"; // Dark Red
            } else {
                ctx.fillStyle = "#ef4444"; // Coral Red
            }
        }
        
        ctx.fill();
        ctx.lineWidth = 3;
        ctx.strokeStyle = "#1e293b";
        ctx.stroke();
        
        // Draw Text
        ctx.save();
        ctx.rotate(sectorAngle + arc / 2);
        ctx.textAlign = "right";
        ctx.fillStyle = "#ffffff";
        ctx.font = "bold 13px 'Plus Jakarta Sans', sans-serif";
        ctx.fillText(sector.label, rWheel - 20, 5);
        ctx.restore();
    }
    
    // 3. Draw Center Pin
    ctx.beginPath();
    ctx.arc(0, 0, 36, 0, Math.PI * 2);
    ctx.fillStyle = "#1e293b";
    ctx.fill();
    ctx.lineWidth = 4;
    ctx.strokeStyle = "#ffd200";
    ctx.stroke();
    
    ctx.beginPath();
    ctx.arc(0, 0, 24, 0, Math.PI * 2);
    ctx.fillStyle = "#ffd200";
    ctx.fill();
    
    ctx.restore();
}

function spinWheel(player) {
    if (isWheelSpinning) return;
    isWheelSpinning = true;
    playSFX('click');
    
    document.getElementById("btn-spin-wheel").disabled = true;
    
    let currentAngle = 0;
    let targetAngle = Math.random() * Math.PI * 2 + Math.PI * 10; // Spin multiple rotations
    let start = null;
    const duration = 4000; // 4 seconds
    
    function animateSpin(timestamp) {
        if (!start) start = timestamp;
        let progress = timestamp - start;
        
        // Ease out cubic function
        let t = progress / duration;
        t = (--t) * t * t + 1; // 1 - (1-t)^3
        
        let angle = currentAngle + t * targetAngle;
        drawWheel(angle);
        
        if (progress < duration) {
            requestAnimationFrame(animateSpin);
        } else {
            // Spinning finished, calculate result sector
            isWheelSpinning = false;
            
            // Normalize angle
            let finalAngle = (angle % (Math.PI * 2));
            
            // Note: pointer points to the top of the wheel (angle -Math.PI / 2)
            const numSectors = WHEEL_SECTORS.length;
            const arc = Math.PI * 2 / numSectors;
            
            // Angle of sector landing is offset from rotation (top of the wheel is Math.PI * 3.5)
            let winningIndex = Math.floor((Math.PI * 3.5 - finalAngle) / arc) % numSectors;
            
            applyWheelResult(player, WHEEL_SECTORS[winningIndex]);
        }
    }
    
    requestAnimationFrame(animateSpin);
}

function applyWheelResult(player, sector) {
    logMessage(`Vòng quay trúng vào: ${sector.label} - ${sector.desc}`);
    
    let resultText = document.getElementById("wheel-result-text");
    resultText.innerHTML = `<span style="color:${sector.type === 'reward' ? 'var(--success)' : 'var(--danger)'};">${sector.label}</span><br><small>${sector.desc}</small>`;
    resultText.style.display = "flex";

    // Apply outcome to state
    if (sector.type === "reward") {
        if (typeof sector.value === "number") {
            let oldIndex = player.tileIndex;
            player.tileIndex = (player.tileIndex + sector.value) % TILE_COORDS.length;
            if (player.tileIndex < oldIndex) {
                player.lapCompleted = true;
                logMessage(`[${player.name}] đã vượt qua vạch xuất phát nhờ phần thưởng từ Vòng Quay!`, "log-win");
            }
        } else if (sector.value === "extra") {
            player.isExtraTurn = true;
        } else if (sector.value === "shield") {
            player.shield = true;
        } else if (sector.value === "double") {
            player.doubleDice = true;
        }
    } else {
        if (typeof sector.value === "number") {
            player.tileIndex = Math.max(0, player.tileIndex + sector.value);
        } else if (sector.value === "skip") {
            player.skipTurn = true;
        } else if (sector.value === "start") {
            player.tileIndex = 0;
        } else if (sector.value === "lose-shield") {
            player.shield = false;
        }
    }

    updatePlayerPositionsOnBoard();
    renderScoreboard();
    playSFX(sector.type === 'reward' ? 'success' : 'fail');

    // Setup next turn / dice roll button
    setTimeout(() => {
        document.getElementById("wheel-modal").classList.remove("active");
        if (sector.value === "extra") {
            // Adjust turn index in offline mode if extra turn won
            if (!isOnlineMode) {
                activePlayerIndex = (activePlayerIndex - 1 + players.length) % players.length;
            }
        }
        nextTurn();
    }, 2000);
}

function startGameTimer(durationMinutes) {
    if (gameTimerInterval) clearInterval(gameTimerInterval);
    
    const timerText = document.getElementById("lblTotalTime") || document.getElementById("game-time-remaining");
    if (!timerText) return;
    
    let remainingSeconds = durationMinutes * 60;
    
    function updateDisplay() {
        const minutes = Math.floor(remainingSeconds / 60);
        const seconds = remainingSeconds % 60;
        timerText.innerText = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        
        if (remainingSeconds <= 60) {
            timerText.style.color = "#e74c3c";
            timerText.style.animation = "blink-warning 1s infinite";
        } else {
            timerText.style.color = "#ffffff";
            timerText.style.animation = "none";
        }
    }
    
    updateDisplay();
    
    gameTimerInterval = setInterval(() => {
        // Auto-pause if any game modal is currently active
        if (document.querySelector('.game-modal.active') || document.querySelector('.game-modal.show')) {
            return;
        }

        remainingSeconds--;
        if (remainingSeconds <= 0) {
            clearInterval(gameTimerInterval);
            timerText.innerText = "Hết giờ!";
            if (isHost && isOnlineMode && connection) {
                connection.invoke("EndGameDueToTimeout", roomCode);
            }
        } else {
            updateDisplay();
        }
    }, 1000);
}

// Victory Screen Trigger
function triggerVictory(winner) {
    localStorage.removeItem("saved_room_code");
    if (gameTimerInterval) {
        clearInterval(gameTimerInterval);
        gameTimerInterval = null;
    }
    const timerCard = document.getElementById("game-timer-card");
    if (timerCard) timerCard.style.display = "none";

    logMessage(`🎉 KÌ TÍCH! Tay đua [${winner.name}] chơi nhân vật [${winner.character.name}] đã chính thức vô địch!`, "log-win");
    
    // Populate winner card
    document.getElementById("winner-avatar").style.backgroundColor = winner.character.color;
    document.getElementById("winner-avatar").innerHTML = winner.character.icon;
    document.getElementById("winner-name").innerText = winner.name;
    document.getElementById("winner-char").innerText = `${winner.character.name} (${winner.character.badge})`;

    // Populate leaderboard table
    const table = document.getElementById("victory-leaderboard-list");
    table.innerHTML = "";

    // Sort by position (lapCount descending, then tileIndex descending, filter out spectators)
    const sorted = [...players]
        .filter(p => !p.isSpectator)
        .sort((a, b) => {
            if (b.lapCount !== a.lapCount) {
                return b.lapCount - a.lapCount;
            }
            return b.tileIndex - a.tileIndex;
        });
    
    sorted.forEach((p, idx) => {
        const row = document.createElement("div");
        row.className = "lb-row";
        
        row.innerHTML = `
            <div class="lb-player">
                <span class="lb-rank lb-rank-${idx + 1}">${idx + 1}</span>
                <span class="lb-avatar" style="background-color: ${p.character.color}">${p.character.icon}</span>
                <span class="lb-name">${p.name}</span>
            </div>
            <div class="lb-score">Vòng ${p.lapCount || 0} - Ô ${p.tileIndex + 1} / ${p.character.name}</div>
        `;
        table.appendChild(row);
    });

    setTimeout(() => {
        showScreen("victory");
    }, 1500);
}

// Helper: Log message to the in-game console
function logMessage(text, className = "") {
    const logs = document.getElementById("game-logs-content");
    if (!logs) return;
    const item = document.createElement("div");
    item.className = className;
    
    // Get timestamp
    const now = new Date();
    const timeStr = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`;
    
    item.innerHTML = `<small style="color:var(--text-secondary); margin-right:5px;">[${timeStr}]</small>${text}`;
    
    logs.appendChild(item);
    
    // Scroll logs to bottom
    logs.scrollTop = logs.scrollHeight;
}
