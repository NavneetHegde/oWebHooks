app.controller("TriggerController",
    ["$scope", '$location', 'NotificationFactory',
    function ($scope, $location, NotificationFactory) {

        $scope.notifyAllProjects = function () {
            NotificationFactory.notification.notifyAll({ events: $scope.selectedFilter });
            $location.path('/trigger');
        };

        $scope.filters = NotificationFactory.filters.get();

    }]);