const characters = [
    {
        img:"https://www.dustloop.com/wiki/images/6/68/BBCF_Valkenhayn_R._Hellsing_Icon.png",
        name: "Valkenhayn",
        frequency: 12.5,
        matches: 1235,
        winrate: 48.5
    }
]


function fill() {
    const matchupContainer = document.getElementById("matchup-container");
    

    for(let i = 0; i < 35; i++){
        const character = characters[0];
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
                            <div class="stat-label">Частота матчапа</div>
                            <div class="stat-value">${character.frequency}%</div>
                        </div>
                        <div class="mainDiv-stat-item">
                            <div class="stat-label">Матчи</div>
                            <div class="stat-value">${character.matches}</div>
                        </div>
                        <div class="mainDiv-stat-item">
                            <div class="stat-label">Винрейт</div>
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