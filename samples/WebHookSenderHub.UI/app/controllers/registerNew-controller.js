app.controller('RegisterNewController',
    ['$scope', 'RegistrationsFactory', '$location',
    function ($scope, RegistrationsFactory, $location) {

        // callback for ng-click 'createNewUser':
        $scope.createNewRegistration = function () {
            RegistrationsFactory.create($scope.registration);
            $location.path('/register');
        }

        $scope.allFilters = ['Insert', 'Update', 'Delete'];

        // callback for ng-click 'cancel':
        $scope.cancel = function () {
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

    }]);