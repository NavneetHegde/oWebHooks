app.controller("RegisterController",
    ["$scope", "$routeParams", "$location", "RegistrationsFactory", "RegistrationFactory",
    function ($scope, $routeParams, $location, RegistrationsFactory, RegistrationFactory) {


        $scope.editRegistration = function (id) {
            $location.path('/editRegister/' + id);
        }

        $scope.createNewRegitration = function () {
            $location.path('/newRegister');
        }

        $scope.deleteRegistration = function (id) {
            RegistrationFactory.delete({ id: id }, null);
            $scope.registrations = RegistrationsFactory.query();
        };

        $scope.registrations = RegistrationsFactory.query();

    }]);