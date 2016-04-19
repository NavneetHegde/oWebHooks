'use strict';

// create the module and name it scotchApp
// also include ngRoute for all our routing needs
var app = angular.module("webHooks",
    ["ngRoute", "ngResource"]
);

// configure  routes
app.config(function ($routeProvider, $locationProvider) {
    $routeProvider
        .when('/register', {
            controller: 'RegisterController',
            templateUrl: 'app/templates/register.html'
        })
        .when('/editRegister/:id', {
            controller: 'RegisterEditController',
            templateUrl: 'app/templates/registrationEdit.html'
        })
        .when('/newRegister', {
            controller: 'RegisterNewController',
            templateUrl: 'app/templates/registrationNew.html'
        })
        .when('/trigger', {
            controller: 'TriggerController',
            templateUrl: 'app/templates/trigger.html'
        })
        .otherwise({
            redirectTo: '/'
        }); //fallback is index.html

    //html5 mode to remove hash from url
    $locationProvider.html5Mode(true);

});


