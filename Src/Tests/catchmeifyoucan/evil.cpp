#include <iostream>

class Thief {
public:
	int Rob() { 
		printf("You are robbed\n"); 
		return 69;
	}
};

int main() {
	Thief* thiefPtr = new Thief();
	printf("thiefPtr: %p", thiefPtr);
	getchar();
}
