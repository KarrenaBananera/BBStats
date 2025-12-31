const body = document.getElementById("tbody");
const pagenumber = document.getElementById("pageInfo");
const next = document.getElementById("nextPage");
const prev = document.getElementById("prevPage");
let currentPage = 1;

for(let i = 0; i < 15; i++){
    body.insertAdjacentHTML(`beforeend`,
        `<tr>
                    <td class="left">${i+1}</th>
                    <td class="left">Mark</td>
                    <td class="right">Jubei</td>
                    <td class="right">2000</td>
        </tr>`
    )
}

function Next(){
    if(currentPage == 5)
        return;

    tbody.innerHTML = '';
    currentPage++;
    const startindex = 15 * (currentPage - 1);

    for(let i = startindex; i < startindex + 15; i++){
        body.insertAdjacentHTML(`beforeend`,
            `<tr>
                <td class="left">${i+1}</th>
                <td class="left">Mark</td>
                <td class="right">Jubei</td>
                <td class="right">2000</td>
            </tr>`
        )
    }

    pagenumber.innerHTML = currentPage;
}

function Prev(){
    console.log(1111111)
    if(currentPage == 1)
        return;

    tbody.innerHTML = '';
    currentPage--;
    const startindex = 15 * (currentPage - 1);

    for(let i = startindex; i < startindex + 15; i++){
        body.insertAdjacentHTML(`beforeend`,
            `<tr>
                <td class="left">${i+1}</th>
                <td class="left">Mark</td>
                <td class="right">Jubei</td>
                <td class="right">2000</td>
            </tr>`
        )
    }

    pagenumber.innerHTML = currentPage;
}

