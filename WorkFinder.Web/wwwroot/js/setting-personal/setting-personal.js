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

let profilePicture = document.getElementById("ImageFile");
let profileImgDisplay = document.getElementById("profile-img-display");

profilePicture.onchange = function () {
    profileImgDisplay.src = window.URL.createObjectURL(this.files[0]);
};

let cvPicture = document.getElementById("CVPicture");
let cvPictureDisplay = document.getElementById("cv-picture-display");

cvPicture.onchange = function () {
    cvPictureDisplay.src = window.URL.createObjectURL(this.files[0]);
};

let tabBasicInfo = document.getElementById("tab-basic-info");
let tabCvResume = document.getElementById("tab-cv-resume");
let basicInfoBlock = document.getElementById("basic-info-block");
let cvResumeBlock = document.getElementById("cv-resume-block");

tabBasicInfo.onclick = (e) => {
    cvResumeBlock.style.display = "none";
    basicInfoBlock.style.display = "block";
    tabBasicInfo.classList.add("tab-action");
    tabCvResume.classList.remove("tab-action");
};

tabCvResume.onclick = (e) => {
    basicInfoBlock.style.display = "none";
    cvResumeBlock.style.display = "block";
    tabCvResume.classList.add("tab-action");
    tabBasicInfo.classList.remove("tab-action");
};
