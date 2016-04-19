app.controller('RegisterEditController',
    ['$scope', '$routeParams', '$location', 'RegistrationFactory',
    function ($scope, $routeParams, $location, RegistrationFactory) {

        $scope.registration = RegistrationFactory.show({ id: $routeParams.id });

        $scope.allFilters = ['Insert', 'Update', 'Delete'];

        // callback for ng-click 'updateUser':
        $scope.updateRegistration = function () {
            RegistrationFactory.update({ id: $routeParams.id }, $scope.registration);
            $location.path('/register');
        };

        $scope.toggleAll = function () {
            if ($scope.isAllSelected) {
                var toggleStatus = !$scope.isAllSelected;
                $scope.registration.Filters = [];
            }
        };

        $scope.toggleSelect = function () {
            $scope.isAllSelected = false;
            var index = $scope.registration.Filters.indexOf("*");
            if (index >= 0) {
                $scope.registration.Filters.splice(index, 1);
            }
            var count = $("input:checkbox[name=eventSelect]:checked").length;
            if (count < 1) {
                $scope.isAllSelected = true;
            }
        };

        // callback for ng-click 'cancel':
        $scope.cancel = function () {
            $location.path('/register');
        };

    }]);