let candidateDashboard = document.getElementById("candidate-dashboard");
let appliedJobs = document.getElementById("applied-jobs");
let favoriteJobs = document.getElementById("favorite-jobs");
let settings = document.getElementById("settings");

candidateDashboard.onclick = (e) => {
    window.location.href = "/Candidate/Dashboard";
};

appliedJobs.onclick = (e) => {
    window.location.href = "/Candidate/AppliedJobs";
};

favoriteJobs.onclick = (e) => {
    window.location.href = "/Candidate/FavoriteJobs";
};

settings.onclick = (e) => {
    window.location.href = "/Candidate/Settings";
};

let jobItemPerPage = 8;
let currentPage = 1;
let jobItems = document.querySelectorAll(".job");
let totalPage = Math.ceil(jobItems.length / jobItemPerPage);

let controlPageNumber = document.querySelector(".control__page-numbers");
let control = document.querySelector(".control");
let controlPrev = document.querySelector(".control__previous");
let controlNext = document.querySelector(".control__next");

showPage(currentPage);
updateControlPageNumber();

function showPage(pageNumber) {
    jobItems.forEach((jobItem, index) => {
        if (
            index >= (pageNumber - 1) * jobItemPerPage &&
            index < pageNumber * jobItemPerPage
        ) {
            setTimeout(() => {
                jobItem.classList.add("show-job");
            }, 100);
            jobItem.style.display = "flex";
        } else {
            jobItem.classList.remove("show-job");
            jobItem.style.display = "none";
        }
    });
}

function updateControlPageNumber() {
    controlPageNumber.innerHTML = "";
    controlNext.classList.remove("control__end-active");
    controlPrev.classList.remove("control__end-active");

    for (let i = 1; i <= totalPage; i++) {
        let pageNumber = document.createElement("div");
        pageNumber.classList.add("control__page-number");
        if (i === currentPage) {
            pageNumber.classList.add("control__page-number--active");
        }
        if (currentPage === 1) {
            controlPrev.classList.add("control__end-active");
        } else if (currentPage === totalPage) {
            controlNext.classList.add("control__end-active");
        }
        pageNumber.innerText = i;
        controlPageNumber.appendChild(pageNumber);
    }
}

control.onclick = function (e) {
    const previousBtn = e.target.closest(".control__previous");
    const nextBtn = e.target.closest(".control__next");
    const pageNumber = e.target.closest(".control__page-number");

    if (previousBtn) {
        if (currentPage > 1) {
            currentPage--;
            showPage(currentPage);
            updateControlPageNumber();
        }
    } else if (nextBtn) {
        if (currentPage < totalPage) {
            currentPage++;
            showPage(currentPage);
            updateControlPageNumber();
        }
    } else if (pageNumber) {
        currentPage = Number(pageNumber.innerText);
        showPage(currentPage);
        updateControlPageNumber();
    }
};
