    

async function fill() {
    const chars_json = await fetch('/data/tager_matchups.json');
    const characters = await chars_json.json();

    const matchupContainer = document.getElementById("matchup-container");

    console.log("Hi");
    console.log(characters);

    for(let i = 0; i < 35; i++){
        const character = characters[i];
        let text;

        if(character.winrate < 45)
            text = "text-danger";
        else{
            if(character.winrate > 55)
                text = "text-success";
            else
                text = "text-warning";
        }

        matchupContainer.insertAdjacentHTML('beforeend',
            `<div class=" col-md-6 col-lg-4">
                <div class="statCard">
                    <div class="character-banner">
                        <img src="${character.img}">
                        <div class="character-name-overlay">
                            <h4 class="">${character.name}</h4>
                        </div>
                    </div>
                    <div class="matchup-stats">
                        <div class="mainDiv-stat-item">
                            <div class="stat-label">Matchup frequency</div>
                            <div class="stat-value">${character.frequency}%</div>
                        </div>
                        <div class="mainDiv-stat-item">
                            <div class="stat-label">Matches</div>
                            <div class="stat-value">${character.matches}</div>
                        </div>
                        <div class="mainDiv-stat-item">
                            <div class="stat-label">Winrate</div>
                            <div class="stat-value ${text}">${character.winrate}%</div>
                        </div>
                    </div>
                </div>
            </div>
            `
        )
    }
}

document.addEventListener('DOMContentLoaded', fill);