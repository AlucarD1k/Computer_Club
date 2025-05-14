// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const menu = document.querySelector(".dropdown-content");
const adminBtn = document.querySelector(".dropbtn").addEventListener('click', function (){
    if (menu.style.display == 'none'){
        menu.style.display = 'flex'
    } else{
        menu.style.display = 'none'
    }
})