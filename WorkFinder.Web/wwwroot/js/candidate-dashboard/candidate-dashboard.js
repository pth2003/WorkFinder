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
