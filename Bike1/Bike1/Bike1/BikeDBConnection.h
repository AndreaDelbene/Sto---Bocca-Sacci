#include "stdafx.h"
//include the below additional libraries
#include <iostream>
#include <windows.h>
#include <sqlext.h>
#include <sqltypes.h>
#include <sql.h>
//use the std namespace
using namespace std;
#pragma once
class BikeDBConnection
{
public:
	BikeDBConnection();
	~BikeDBConnection();
	void createConnection();
};

